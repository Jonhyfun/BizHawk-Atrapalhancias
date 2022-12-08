using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizHawk.Client.EmuHawk.Jotas.Games
{
    public abstract class BaseAtrapalhanciaGame : IAtrapalhanciaGame
	{
		public abstract string Watch { get; }

		public static LuaConsole Lua = null;

		public virtual void OnLoad()
		{
			Lua = (LuaConsole)MainForm.Instance.Tools.Get<LuaConsole>(); //todo JOTAS tirar essa gambiarra e a do ramwatch
			Lua.WindowState = System.Windows.Forms.FormWindowState.Minimized;
		}
	}
}
