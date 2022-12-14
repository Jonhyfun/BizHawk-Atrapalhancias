using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BizHawk.Client.EmuHawk.Jotas.Games;

namespace BizHawk.Client.EmuHawk.Jotas.Utils
{
	public class AtrapalhanciaBuilder
	{
		public static Dictionary<string, Action<dynamic>> ExposeAtrapalhancias(Type type)
		{
			var result = new Dictionary<string, Action<dynamic>>();
			var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public).ToList();
			methods.AddRange(typeof(Shared).GetMethods(BindingFlags.Static | BindingFlags.Public));

            foreach (MethodInfo action in methods)
			{
				result.Add(action.Name, (arg) => 
				{
					try
					{
						action.Invoke(null, new object[] { arg });
					}
					catch
					{
						action.Invoke(null, null);
					}
				});
			}

			return result;
		}
	}
}
