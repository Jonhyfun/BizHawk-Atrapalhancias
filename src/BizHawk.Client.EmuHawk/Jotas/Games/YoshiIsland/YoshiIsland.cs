using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk.Jotas.Utils;
using BizHawk.Emulation.Cores.Nintendo.SNES9X;

namespace BizHawk.Client.EmuHawk.Jotas.Games.YoshiIsland
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method", Justification = "<Pendente>")]
    public class YoshiIsland : BaseAtrapalhanciaGame
	{

		#region YoshiIsland NOT_Atrapalhancias

		private static string[] addressPointers = { "0", "4", "8", "C" };
		private static string firstCharAddress = "4E00";

		public override string Watch { get => $"{Directory.GetCurrentDirectory()}\\Jotas\\Games\\YoshiIsland\\currentWatch.wch"; }

		private static void GetHalf(string pointer, List<string> addressess)
		{
			for (int i = 0; i < 1; i++)
			{
				string newPointer = Hex.NumberToHexCartRAM(Hex.HexCartRAMToNumber(pointer) + i * 16);
				string newPointerBase = StringHandler.TakeString(newPointer, 3);
				foreach (string addressPointer in addressPointers)
				{
					addressess.Add($"{newPointerBase}{addressPointer}");
				}
			}
		}

		private static List<string> GetLetterAddresses(int letterIndex)
		{
			int currentLetterIndex = letterIndex == 0 ? 0 : (letterIndex) * 16;
			string addressPointer = new string(((IEnumerable<char>)firstCharAddress.Clone()).ToArray());
			string[] addressPointerHalfs = new string[2];

			addressPointerHalfs[0] = Hex.NumberToHexCartRAM(Hex.HexCartRAMToNumber(addressPointer) + currentLetterIndex);
			addressPointerHalfs[1] = Hex.NumberToHexCartRAM(Hex.HexCartRAMToNumber(addressPointer) + 256 + currentLetterIndex);

			List<string> addressess = new List<string>();

			foreach (string half in addressPointerHalfs)
			{
				GetHalf(half, addressess);
			}

			return addressess;

		}

		private static Dictionary<string,string> GetLetterDrawingAddressDict(int letterIndex, char letter)
		{
			var resultingDict = new Dictionary<string, string>();
			var letterMap = MessageBoxSymbols.Letters.Keys.Contains(letter) ? MessageBoxSymbols.Letters[letter] : MessageBoxSymbols.Letters[' '];

			var letterAddresses = GetLetterAddresses(letterIndex);

			for (int i = 0; i < letterAddresses.Count; i++)
			{
				var address = letterAddresses[i];
				if (address.ToCharArray()[2] == 'F')
				{
					resultingDict.Add(address, "00FF00FF");
				}
				else if (letterMap.Length > i)
				{
					resultingDict.Add(address, letterMap[i]);
				} 
			}

			return resultingDict;
		}

		private static string ReverseBytes(string DWord)
		{
			var resultingStringList = new List<string>();
			for (int i = 0; i < DWord.Length - 1; i += 2)
			{
				resultingStringList.Add(new string(DWord.Substring(i, 2).Reverse().ToArray()));
			}
			return string.Join("",resultingStringList);
		}

		private static void MountLetterToBuffer(List<KeyValuePair<string,string>> bufferDict, int letterIndex, char letter)
		{
			foreach (var letterKVP in GetLetterDrawingAddressDict(letterIndex, letter))
			{
				bufferDict.Add(new KeyValuePair<string,string>(letterKVP.Key, ReverseBytes(new string(letterKVP.Value.Reverse().ToArray()))));
			}
		}

		private static void WriteText(string word)
		{
			if (word == null || word.Length == 0) return;
			var PhraseDict = new List<KeyValuePair<string,string>>();
			word = StringHandler.RemoverAcentos(HttpUtility.UrlDecode(word.Split('=')[1].Replace("+", " ")));
			var letters = word.ToCharArray();
			var lettersWithSpace = new List<char>();
			var wordBreakCounter = 0;

			for (int i = 0; i < letters.Length; i++)
			{
				wordBreakCounter++;
				lettersWithSpace.Add(letters[i]);

				if(wordBreakCounter == 15)
				{
					wordBreakCounter = 0;
					for (int j = 0; j < 17; j++)
					{
						lettersWithSpace.Add(' ');
					}
				}
			}

			for (int i = 0; i < lettersWithSpace.ToArray().Length; i++)
			{
				MountLetterToBuffer(PhraseDict, i, lettersWithSpace[i]);
			}
			var unfreeze = RamWatch.ram.FreezeCartRAMRange(PhraseDict);

			Task.Delay(500).Wait();
			unfreeze();
		}

		#endregion

		public override void OnLoad()
		{
			base.OnLoad();
			RamWatch.ram.Poke("babyHp", "01");
		}

		public static Task HitYoshi(string word)
		{
			RamWatch.ram.Poke("yoshiHitState", "A0");

			Task.Delay(350).Wait();

			word = "=ABAAAAAAAAAAAAAAAAAAAAAAAAAACATE";

			if (word != null)
			{
				Shared.MessageBox(word).Wait();
			}

			return Task.CompletedTask;
		}

		public static Task HoldDown()
		{
			var unfreeze = RamWatch.ram.Freeze("currentArrow", 04);
			Task.Delay(4000).Wait();
			unfreeze();

			return Task.CompletedTask;
		}

		public static Task Jump()
		{
			var unfreeze = Snes9x.FreezeButton("0B");
			Task.Delay(1000).Wait();
			unfreeze();

			Task.Delay(700).Wait();
			return Task.CompletedTask;
		}

		public static Task UltraSpeed()
		{
			var unfreeze = RamWatch.ram.Freeze("xVelocity", 255);
			Task.Delay(5000).Wait();
			unfreeze();

			return Task.CompletedTask;
		}

		public static Task Rewind()
		{
			MainForm.ForceRewind = true;
			Task.Delay(5000).Wait();
			MainForm.ForceRewind = false;

			return Task.CompletedTask;
		}

		public static Task BlackScreen() 
		{
			Lua.RunLuaAction("gui.drawBox(0,0,client.bufferwidth(),client.bufferheight(), 'black', 'black')");
			Task.Delay(5000).Wait();
			Lua.RunLuaAction("gui.drawBox(0,0,0,0, 'black', 'black')");

			return Task.CompletedTask;
		}

		public override void UpdateHook(string note, byte value, int previous, Action<string> Poke)
		{
            if (note.ToLower() == "babyhp")
            {
                if (value > 17) Poke("10");
            }

            if (note.ToLower() == "lives")
            {
                if (previous != 0 && value < previous)
                {
                    var _value = int.Parse(value.ToString());
                    if (
						_value == 2)
                    {
                        Console.WriteLine(MainForm.Instance.Player + " morreu");
                        var content = new FormUrlEncodedContent(new Dictionary<string, string> { { "died", MainForm.Instance.Player } });
                        try
                        {
                            HttpCommunication._client.PostAsync("http://localhost:8000/fromSnes", content);
                            value = 3;
                            Poke("03");
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
	}
}
