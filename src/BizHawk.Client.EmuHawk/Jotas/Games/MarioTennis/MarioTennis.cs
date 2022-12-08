using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BizHawk.Client.Common;

namespace BizHawk.Client.EmuHawk.Jotas.Games.MarioTennis
{
	public class MarioTennis : BaseAtrapalhanciaGame
    {
		public override string Watch => $"{Directory.GetCurrentDirectory()}\\Jotas\\Games\\MarioTennis\\currentWatch.wch";

		public override void OnLoad()
		{
            base.OnLoad();
            Lua = (LuaConsole)MainForm.Instance.Tools.Get<LuaConsole>(); //todo JOTAS tirar essa gambiarra e a do ramwatch
			Lua.WindowState = System.Windows.Forms.FormWindowState.Minimized;
		}

		public static Task FreezeBall()
		{
			var unfreezeX = RamWatch.ram.Freeze("X", RamWatch.ram.Get("X"));
			var unfreezeY = RamWatch.ram.Freeze("X", RamWatch.ram.Get("Y"));
			
			Task.Delay(4000).Wait();
			
			unfreezeX();
			unfreezeY();

			return Task.CompletedTask;
		}

		public static Task MessageBox(string word)
		{
			var textList = new List<char>();
			var textArray = HttpUtility.UrlDecode(word.Split('=')[1].Replace("+", " ").Replace("\n", " ")).ToCharArray();

			for (int i = 0; i < textArray.Length; i++)
			{
				if (i == 1)
				{
					textList.Add(textArray[i]);
					continue;
				}
				if ((i % 25) == 1)
				{
					textList.Add('\\');
					textList.Add('n');
				}
				textList.Add(textArray[i]);

			}

			var text = new string(textList.ToArray());

			new Thread(() =>
			{
				Lua.RunLuaAction($"gui.clearGraphics()");
				Lua.RunLuaAction($"gui.clearGraphics('client')");
				Lua.RunLuaAction($"gui.drawText(120, 76, '{text}', nil, 'black', 22, NIL, NIL, NIL, NIL, 'client')");
				Lua.RunLuaAction($"gui.drawBox(50, 35, 210, 175, nil, 'black')");
			}).Start();

			var time = text.Length / 2 * 150;

			time = time < 4000 ? 4000 : time;

			Task.Delay(time).Wait();

			Lua.RunLuaAction($"gui.clearGraphics()");
			Lua.RunLuaAction($"gui.clearGraphics('client')");

			Task.Delay(750).Wait();

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
			try
			{
				Lua.RunLuaAction("gui.drawBox(0,0,1000,1000, 'black', 'black')");
				Task.Delay(5000).Wait();
				Lua.RunLuaAction("gui.drawBox(0,0,0,0, 'black', 'black')");
			}
			catch
			{

			} 

			return Task.CompletedTask;
		}
	}
}
