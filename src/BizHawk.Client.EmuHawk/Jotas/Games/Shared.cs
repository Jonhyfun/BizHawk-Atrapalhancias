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
				Lua.RunLuaAction($"gui.pixelText(((client.bufferwidth() / 2))-((client.bufferwidth() / 4))-20, ((client.bufferheight() / 2)-(client.bufferheight() / 3))-20, 'white', 'black', 0)");
			}).Start();

			var time = text.Length / 2 * 150;

			time = time < 4000 ? 4000 : time;

			Task.Delay(time).Wait();

			Lua.RunLuaAction($"gui.clearGraphics()");

			Task.Delay(750).Wait();

			return Task.CompletedTask;

		}

		public override void UpdateHook(string note, byte value, int previous, Action<string> Poke)
		{
			
		}
	}
}
