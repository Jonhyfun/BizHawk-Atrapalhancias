using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizHawk.Client.EmuHawk.Jotas.Games
{
	public interface IAtrapalhanciaGame
	{
		public string Watch { get; }
		public void OnLoad();
	}
}
