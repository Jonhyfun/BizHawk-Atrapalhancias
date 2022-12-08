using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizHawk.Client.EmuHawk.Jotas.Utils
{
	public class Generators
	{
		public static string RandomHexString()
		{
			// 64 character precision or 256-bits
			Random rdm = new Random();
			string hexValue = string.Empty;
			int num;

			for (int i = 0; i < 8; i++)
			{
				num = rdm.Next(0, int.MaxValue);
				hexValue += num.ToString("X8");
			}

			return hexValue;
		}
	}
}
