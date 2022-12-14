﻿using System;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Waterbox;
using System.IO;
using System.ComponentModel;
using BizHawk.Common;
using System.Collections.Generic;
using System.Linq;

using BizHawk.Emulation.Cores.Nintendo.SNES;

namespace BizHawk.Emulation.Cores.Nintendo.SNES9X
{
	[PortedCore(CoreNames.Snes9X, "", "5e0319ab3ef9611250efb18255186d0dc0d7e125", "https://github.com/snes9xgit/snes9x")]
	[ServiceNotApplicable(new[] { typeof(IDriveLight) })]
	public class Snes9x : WaterboxCore,
		ISettable<Snes9x.Settings, Snes9x.SyncSettings>, IRegionable
	{
		private readonly LibSnes9x _core;
		private static Snes9x Instance;

		[CoreConstructor(VSystemID.Raw.SNES)]
		public Snes9x(CoreComm comm, byte[] rom, Settings settings, SyncSettings syncSettings)
			: base(comm, new Configuration
			{
				DefaultWidth = 256,
				DefaultHeight = 224,
				MaxWidth = 512,
				MaxHeight = 480,
				MaxSamples = 8192,
				SystemId = VSystemID.Raw.SNES,
			})
		{
			settings ??= new Settings();
			syncSettings ??= new SyncSettings();

			_core = PreInit<LibSnes9x>(new WaterboxOptions
			{
				Filename = "snes9x.wbx",
				SbrkHeapSizeKB = 1024,
				SealedHeapSizeKB = 12 * 1024,
				InvisibleHeapSizeKB = 6 * 1024,
				PlainHeapSizeKB = 12 * 1024,
				SkipCoreConsistencyCheck = comm.CorePreferences.HasFlag(CoreComm.CorePreferencesFlags.WaterboxCoreConsistencyCheck),
				SkipMemoryConsistencyCheck = comm.CorePreferences.HasFlag(CoreComm.CorePreferencesFlags.WaterboxMemoryConsistencyCheck),
			});

			if (!_core.biz_init())
				throw new InvalidOperationException("Init() failed");
			if (!_core.biz_load_rom(rom, rom.Length))
				throw new InvalidOperationException("LoadRom() failed");

			PostInit();

			if (_core.biz_is_ntsc())
			{
				Console.WriteLine("NTSC rom loaded");
				VsyncNumerator = 21477272;
				VsyncDenominator = 357366;
				Region = DisplayType.NTSC;
			}
			else
			{
				Console.WriteLine("PAL rom loaded");
				VsyncNumerator = 21281370;
				VsyncDenominator = 425568;
				Region = DisplayType.PAL;
			}

			_syncSettings = syncSettings;
			InitControllers();
			PutSettings(settings);
			Instance = this;
		}

		private readonly short[] _inputState = new short[16 * 8];
		private List<ControlDefUnMerger> _cdums;
		private readonly List<IControlDevice> _controllers = new List<IControlDevice>();

		private void InitControllers()
		{
			_core.biz_set_port_devices(_syncSettings.LeftPort, _syncSettings.RightPort);

			switch (_syncSettings.LeftPort)
			{
				case LibSnes9x.LeftPortDevice.Joypad:
					_controllers.Add(new Joypad());
					break;
			}
			switch (_syncSettings.RightPort)
			{
				case LibSnes9x.RightPortDevice.Joypad:
					_controllers.Add(new Joypad());
					break;
				case LibSnes9x.RightPortDevice.Multitap:
					_controllers.Add(new Joypad());
					_controllers.Add(new Joypad());
					_controllers.Add(new Joypad());
					_controllers.Add(new Joypad());
					break;
				case LibSnes9x.RightPortDevice.Mouse:
					_controllers.Add(new Mouse());
					break;
				case LibSnes9x.RightPortDevice.SuperScope:
					_controllers.Add(new SuperScope());
					break;
				case LibSnes9x.RightPortDevice.Justifier:
					_controllers.Add(new Justifier());
					break;
			}

			_controllerDefinition = ControllerDefinitionMerger.GetMerged(
				"SNES Controller",
				_controllers.Select(c => c.Definition),
				out _cdums);

			// add buttons that the core itself will handle
			_controllerDefinition.BoolButtons.Add("Reset");
			_controllerDefinition.BoolButtons.Add("Power");

			_controllerDefinition.MakeImmutable();
		}

		private static Random rng = new Random();

		public static IList<T> ShuffleList<T>(IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			return list;
		}

		public static IDictionary<TK, TV> Shuffle<TK, TV>(IDictionary<TK, TV> dictionary)
		{
			var values = ShuffleList(dictionary.Values.ToArray());
			for (int i = 0; i < values.Count; i++)
			{
				dictionary[dictionary.Keys.ToArray()[i]] = values[i];
			}
			return dictionary;
		}

		public static IDictionary<string, string> OriginalControls = null;
		public static void InvertCross()
		{
			if (OriginalControls == null) OriginalControls = new Dictionary<string, string>(Instance._cdums[0]._buttonAxisRemaps);
			var leftKey = Instance._cdums[0]._buttonAxisRemaps.First((k) => k.Value == "P1 Left").Key;
			var rightKey = Instance._cdums[0]._buttonAxisRemaps.First((k) => k.Value == "P1 Right").Key;
			var upKey = Instance._cdums[0]._buttonAxisRemaps.First((k) => k.Value == "P1 Up").Key;
			var downKey = Instance._cdums[0]._buttonAxisRemaps.First((k) => k.Value == "P1 Down").Key;

			Instance._cdums[0]._buttonAxisRemaps[leftKey] = "P1 Right";
			Instance._cdums[0]._buttonAxisRemaps[rightKey] = "P1 Left";
			Instance._cdums[0]._buttonAxisRemaps[upKey] = "P1 Down";
			Instance._cdums[0]._buttonAxisRemaps[downKey] = "P1 Up";
		}

		public static void ShuffleControls()
		{
			if (OriginalControls == null) OriginalControls = new Dictionary<string, string>(Instance._cdums[0]._buttonAxisRemaps);
			Instance._controllers[0].Definition._buttons = ShuffleList(Instance._controllers[0].Definition._buttons);
			Instance._cdums[0]._buttonAxisRemaps = Shuffle(Instance._cdums[0]._buttonAxisRemaps);
		}

		public static Action FreezeButton(string button)
		{
			if (Joypad.JotasPressedJoyButtons.ContainsKey(button))
			{
				Joypad.JotasPressedJoyButtons[button] = true;
				return () => Joypad.JotasPressedJoyButtons[button] = false;
			}
			return () => { };
		}

        public static Action FreezeUnButton(string button)
        {
            if (Joypad.JotasUnPressedJoyButtons.ContainsKey(button))
            {
                Joypad.JotasUnPressedJoyButtons[button] = true;
                return () => Joypad.JotasUnPressedJoyButtons[button] = false;
            }
            return () => { };
        }

        public static string GetRandomJoyButton()
		{
			return Joypad.Buttons[rng.Next(0, Joypad.Buttons.Length - 1)];
		}

		public static string GetRandomJoyButton(string ignore)
		{
			var notIgnoredButtons = Joypad.Buttons.Where(w => w != ignore).ToArray();
			return notIgnoredButtons[rng.Next(0, notIgnoredButtons.Length - 1)];
		}

		public static void UndoJotasControllerEdit()
		{
			Instance._cdums[0]._buttonAxisRemaps = new Dictionary<string, string>(OriginalControls);
		}

		private void UpdateControls(IController c)
		{
			Array.Clear(_inputState, 0, 16 * 8);
			for (int i = 0, offset = 0; i < _controllers.Count; i++, offset += 16)
			{
				_controllers[i].ApplyState(_cdums[i].UnMerge(c), _inputState, offset);
			}
		}

		private interface IControlDevice
		{
			ControllerDefinition Definition { get; }
			void ApplyState(IController controller, short[] input, int offset);
		}

		private class Joypad : IControlDevice
		{
			public static readonly string[] Buttons =
			{
				"0B",
				"0Y",
				"0Select",
				"0Start",
				"0Up",
				"0Down",
				"0Left",
				"0Right",
				"0A",
				"0X",
				"0L",
				"0R"
			};

			private static int ButtonOrder(string btn)
			{
				var order = new Dictionary<string, int>
				{
					["0Up"] = 0,
					["0Down"] = 1,
					["0Left"] = 2,
					["0Right"] = 3,

					["0Select"] = 4,
					["0Start"] = 5,

					["0Y"] = 6,
					["0B"] = 7,

					["0X"] = 8,
					["0A"] = 9,

					["0L"] = 10,
					["0R"] = 11
				};

				return order[btn];
			}

			public static Dictionary<string, bool> JotasPressedJoyButtons = new Dictionary<string, bool>
			{
				["0Up"] = false,
				["0Down"] = false,
				["0Left"] = false,
				["0Right"] = false,

				["0Select"] = false,
				["0Start"] = false,

				["0Y"] = false,
				["0B"] = false,

				["0X"] = false,
				["0A"] = false,

				["0L"] = false,
				["0R"] = false
			};

            public static Dictionary<string, bool> JotasUnPressedJoyButtons = new Dictionary<string, bool>
            {
                ["0Up"] = false,
                ["0Down"] = false,
                ["0Left"] = false,
                ["0Right"] = false,

                ["0Select"] = false,
                ["0Start"] = false,

                ["0Y"] = false,
                ["0B"] = false,

                ["0X"] = false,
                ["0A"] = false,

                ["0L"] = false,
                ["0R"] = false
            };

            private static readonly ControllerDefinition _definition = new("(SNES Controller fragment)")
			{
				BoolButtons = Buttons.OrderBy(ButtonOrder).ToList()
			};

			public ControllerDefinition Definition { get; } = _definition;

			public void ApplyState(IController controller, short[] input, int offset)
			{
				for (int i = 0; i < Buttons.Length; i++)
					input[offset + i] = (short)((JotasPressedJoyButtons[Buttons[i]] || (JotasUnPressedJoyButtons[Buttons[i]] == true ? false : controller.IsPressed(Buttons[i]))) ? 1 : 0);
			}
		}

		private abstract class Analog : IControlDevice
		{
			public abstract ControllerDefinition Definition { get; }

			public void ApplyState(IController controller, short[] input, int offset)
			{
				foreach (var s in Definition.Axes.Keys)
					input[offset++] = (short)(controller.AxisValue(s));
				foreach (var s in Definition.BoolButtons)
					input[offset++] = (short)(controller.IsPressed(s) ? 1 : 0);
			}
		}

		private class Mouse : Analog
		{
			private static readonly ControllerDefinition _definition
				= new ControllerDefinition("(SNES Controller fragment)") { BoolButtons = { "0Mouse Left", "0Mouse Right" } }
					.AddXYPair("0Mouse {0}", AxisPairOrientation.RightAndUp, (-127).RangeTo(127), 0); //TODO verify direction against hardware

			public override ControllerDefinition Definition => _definition;
		}

		private class SuperScope : Analog
		{
			private static readonly ControllerDefinition _definition
				= new ControllerDefinition("(SNES Controller fragment)") { BoolButtons = { "0Trigger", "0Cursor", "0Turbo", "0Pause" } }
					.AddLightGun("0Scope {0}");

			public override ControllerDefinition Definition => _definition;
		}

		private class Justifier : Analog
		{
			private static readonly ControllerDefinition _definition
				= new ControllerDefinition("(SNES Controller fragment)") { BoolButtons = { "0Trigger", "0Start" } }
					.AddLightGun("0Justifier {0}");

			public override ControllerDefinition Definition => _definition;
		}

		private ControllerDefinition _controllerDefinition;
		public override ControllerDefinition ControllerDefinition => _controllerDefinition;

		public DisplayType Region { get; }

		protected override LibWaterboxCore.FrameInfo FrameAdvancePrep(IController controller, bool render, bool rendersound)
		{
			if (controller.IsPressed("Power"))
				_core.biz_hard_reset();
			else if (controller.IsPressed("Reset"))
				_core.biz_soft_reset();
			UpdateControls(controller);
			_core.SetButtons(_inputState);

			return new LibWaterboxCore.FrameInfo();
		}

		public override int VirtualWidth => BufferWidth == 256 && BufferHeight <= 240 ? 293 : 587;
		public override int VirtualHeight => BufferHeight <= 240 && BufferWidth == 512 ? BufferHeight * 2 : BufferHeight;

		protected override void LoadStateBinaryInternal(BinaryReader reader)
		{
			_core.biz_post_load_state();
		}

		private Settings _settings;
		private SyncSettings _syncSettings;

		public Settings GetSettings()
		{
			return _settings.Clone();
		}

		public SyncSettings GetSyncSettings()
		{
			return _syncSettings.Clone();
		}

		public PutSettingsDirtyBits PutSettings(Settings o)
		{
			_settings = o;
			int s = 0;
			if (o.PlaySound0) s |= 0b1;
			if (o.PlaySound1) s |= 0b10;
			if (o.PlaySound2) s |= 0b100;
			if (o.PlaySound3) s |= 0b1000;
			if (o.PlaySound4) s |= 0b10000;
			if (o.PlaySound5) s |= 0b100000;
			if (o.PlaySound6) s |= 0b1000000;
			if (o.PlaySound7) s |= 0b10000000;
			_core.biz_set_sound_channels(s);
			int l = 0;
			if (o.ShowBg0) l |= 1;
			if (o.ShowBg1) l |= 2;
			if (o.ShowBg2) l |= 4;
			if (o.ShowBg3) l |= 8;
			if (o.ShowWindow) l |= 32;
			if (o.ShowTransparency) l |= 64;
			if (o.ShowSprites0) l |= 256;
			if (o.ShowSprites1) l |= 512;
			if (o.ShowSprites2) l |= 1024;
			if (o.ShowSprites3) l |= 2048;
			_core.biz_set_layers(l);

			return PutSettingsDirtyBits.None; // no reboot needed
		}

		public PutSettingsDirtyBits PutSyncSettings(SyncSettings o)
		{
			var ret = SyncSettings.NeedsReboot(_syncSettings, o);
			_syncSettings = o;
			return ret ? PutSettingsDirtyBits.RebootCore : PutSettingsDirtyBits.None;
		}

		public class Settings
		{
			[DefaultValue(true)]
			[DisplayName("Enable Sound Channel 1")]
			public bool PlaySound0 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Sound Channel 2")]
			public bool PlaySound1 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Sound Channel 3")]
			public bool PlaySound2 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Sound Channel 4")]
			public bool PlaySound3 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Sound Channel 5")]
			public bool PlaySound4 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Sound Channel 6")]
			public bool PlaySound5 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Sound Channel 7")]
			public bool PlaySound6 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Sound Channel 8")]
			public bool PlaySound7 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Background Layer 1")]
			public bool ShowBg0 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Background Layer 2")]
			public bool ShowBg1 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Background Layer 3")]
			public bool ShowBg2 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Enable Background Layer 4")]
			public bool ShowBg3 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Show Sprites Priority 1")]
			public bool ShowSprites0 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Show Sprites Priority 2")]
			public bool ShowSprites1 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Show Sprites Priority 3")]
			public bool ShowSprites2 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Show Sprites Priority 4")]
			public bool ShowSprites3 { get; set; }

			[DefaultValue(true)]
			[DisplayName("Show Window")]
			public bool ShowWindow { get; set; }

			[DefaultValue(true)]
			[DisplayName("Show Transparency")]
			public bool ShowTransparency { get; set; }

			public Settings()
			{
				SettingsUtil.SetDefaultValues(this);
			}

			public Settings Clone()
			{
				return (Settings)MemberwiseClone();
			}
		}

		public class SyncSettings
		{
			[DefaultValue(LibSnes9x.LeftPortDevice.Joypad)]
			[DisplayName("Left Port")]
			[Description("Specifies the controller type plugged into the left controller port on the console")]
			public LibSnes9x.LeftPortDevice LeftPort { get; set; }

			[DefaultValue(LibSnes9x.RightPortDevice.Joypad)]
			[DisplayName("Right Port")]
			[Description("Specifies the controller type plugged into the right controller port on the console")]
			public LibSnes9x.RightPortDevice RightPort { get; set; }

			public SyncSettings()
			{
				SettingsUtil.SetDefaultValues(this);
			}

			public SyncSettings Clone()
			{
				return (SyncSettings)MemberwiseClone();
			}

			public static bool NeedsReboot(SyncSettings x, SyncSettings y)
			{
				// the core can handle dynamic plugging and unplugging, but that changes
				// the controllerdefinition, and we're not ready for that
				return !DeepEquality.DeepEquals(x, y);
			}
		}
	}
}
