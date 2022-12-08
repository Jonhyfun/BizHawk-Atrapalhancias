using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizHawk.Client.EmuHawk.Jotas.Utils
{
	public class StringHandler
	{
		public static string TakeString(string text, int length)
		{
			return new string(text.Take(length).ToArray());
		}

		public static string RemoverAcentos(string texto)
		{
			string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
			string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

			for (int i = 0; i < comAcentos.Length; i++)
			{
				texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
			}
			return texto;
		}
	}
}
