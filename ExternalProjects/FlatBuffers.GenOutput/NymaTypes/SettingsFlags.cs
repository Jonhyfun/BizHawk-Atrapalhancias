// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace NymaTypes
{

[System.FlagsAttribute]
public enum SettingsFlags : uint
{
  /// TODO(cats)
  Input = 256,
  Sound = 512,
  Video = 1024,
  /// User-configurable physical->virtual button/axes and hotkey mappings(driver-side code category mainly).
  InputMapping = 2048,
  Path = 4096,
  /// If the setting affects emulation from the point of view of the emulated program
  EmuState = 131072,
  /// If it's safe for an untrusted source to modify it, probably only used in conjunction with MDFNST_EX_EMU_STATE and network play
  UntrustedSafe = 262144,
  /// Suppress documentation generation for this setting.
  SuppressDoc = 524288,
  /// Auto-generated common template setting(like nes.xscale, pce.xscale, vb.xscale, nes.enable, pce.enable, vb.enable)
  CommonTemplate = 1048576,
  /// Don't save setting in settings file.
  NonPersistent = 2097152,
  /// TODO(in progress)
  RequiresReload = 16777216,
  RequiresRestart = 33554432,
};


}
