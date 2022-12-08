using System;

namespace BizHawk.Client.EmuHawk.Jotas.Games
{
	public interface IAtrapalhanciaGame
	{
		public string Watch { get; }
		public void OnLoad();
		public void UpdateHook(string note, byte value, int previous, Action<string> Poke);

    }
}
