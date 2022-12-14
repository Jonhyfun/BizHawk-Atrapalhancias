using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BizHawk.Client.Common;
using BizHawk.Emulation.Cores.Nintendo.SNES9X;

namespace BizHawk.Client.EmuHawk.Jotas.Games.DKC3
{
	public class DKC3 : BaseAtrapalhanciaGame
	{
		public override string Watch => $"{Directory.GetCurrentDirectory()}\\Jotas\\Games\\DKC3\\currentWatch.wch";

		public override void OnLoad()
		{
			base.OnLoad();
		}

        public static Task Jump()
        {
            var unfreeze = Snes9x.FreezeButton("0B");
            var unfreezeX = Snes9x.FreezeUnButton("0Y");

            Task.Delay(950).Wait();

            unfreeze();
            unfreezeX();

            return Task.CompletedTask;
        }

        public static Task Back()
		{
			var unfreeze = Snes9x.FreezeButton("0Left");
			var unfreezeX = Snes9x.FreezeUnButton("0Right");

			Task.Delay(3000).Wait();

			unfreeze();
			unfreezeX();

            return Task.CompletedTask;
		}

        public static Task Map()
        {
            var unfreeze = Snes9x.FreezeButton("0Start");
            Task.Delay(300).Wait();
            unfreeze();

            unfreeze = Snes9x.FreezeButton("0Select");
            Task.Delay(300).Wait();
            unfreeze();

            return Task.CompletedTask;
        }

		public static Task Speed() 
		{
			MainForm.ForceTurbo = true;
            Task.Delay(3000).Wait();
            MainForm.ForceTurbo = false;
            return Task.CompletedTask;
		}


        public override void UpdateHook(string note, byte value, int previous, Action<string> Poke)
		{
            if (note.ToLower() == "lives")
            {
                if (previous != 0 && value < previous)
                {
                    if (value != 98)
                    {
                        RamWatch.ram.Poke("lives", "99");
                    }
                    else
                    {
                        var _value = int.Parse(value.ToString());
                        if (_value == 98)
                        {
                            Console.WriteLine(MainForm.Instance.Player + " morreu");
                            var content = new FormUrlEncodedContent(new Dictionary<string, string> { { "died", MainForm.Instance.Player } });
                            try
                            {
                                HttpCommunication._client.PostAsync("http://localhost:8000/fromSnes", content);
                                value = 3;
                                Poke("99");
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
}
