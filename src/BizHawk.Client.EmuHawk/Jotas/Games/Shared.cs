using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BizHawk.Client.EmuHawk.Jotas.Games
{
	public class Shared : BaseAtrapalhanciaGame
    {

		public override string Watch => "";


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

		public static Task Nothing() 
		{
			return Task.CompletedTask;
		}

        public override void UpdateHook(string note, byte value, int previous, Action<string> Poke)
		{
			
		}
	}
}
