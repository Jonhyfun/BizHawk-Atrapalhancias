using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizHawk.Client.EmuHawk.Jotas.Utils
{
	public class Hex
	{
		public static string NumberToHexCartRAM(int hex)
		{
			return hex.ToString("X4");
		}

		public static int HexCartRAMToNumber(string hex)
		{
			return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
		}
	}
}
