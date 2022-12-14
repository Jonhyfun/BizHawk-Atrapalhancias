using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BizHawk.Client.Common;
using BizHawk.Emulation.Cores.Nintendo.SNES9X;

namespace BizHawk.Client.EmuHawk.Jotas.Games.Metroid
{
	public class Metroid : BaseAtrapalhanciaGame
	{
        private static Random rnd = new Random();
		public override string Watch => $"{Directory.GetCurrentDirectory()}\\Jotas\\Games\\Metroid\\currentWatch.wch";

		public override void OnLoad()
		{
			base.OnLoad();
		}

        public static Task Jump()
        {
            var unfreeze = Snes9x.FreezeButton("0A");

            Task.Delay(950).Wait();

            unfreeze();

            return Task.CompletedTask;
        }

        public static Task Escorregar()
		{
            var times = rnd.Next(5,8);

            while(times > 0)
            {
                times--;
                var timer = rnd.Next(200, 700);

                var direction = RamWatch.ram.Get("direction");

                var unfreeze = RamWatch.ram.Freeze("direction", direction == 4 ? 8 : 4);

			    Task.Delay(timer).Wait();
                unfreeze();
                Task.Delay(500).Wait();
            }

            return Task.CompletedTask;
		}

        public static Task NoBullet()
        {
            var unfreeze = RamWatch.ram.Freeze("cooldown", 10);

            Task.Delay(3000).Wait();

            unfreeze();

            return Task.CompletedTask;
        }

        public static Task SlowSpeed()
        {
            var unfreeze = RamWatch.ram.Freeze("moveSpeed", 0);

            Task.Delay(5000).Wait();

            unfreeze();

            return Task.CompletedTask;
        }

        public static Task GlitchJump()
        {
            var unfreeze = RamWatch.ram.Freeze("bugarPulo", 0);

            Task.Delay(3500).Wait();

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

        public static Task UltraRewind()
        {
            MainForm.ForceRewind = true;
            Task.Delay(60000).Wait();
            MainForm.ForceRewind = false;
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
