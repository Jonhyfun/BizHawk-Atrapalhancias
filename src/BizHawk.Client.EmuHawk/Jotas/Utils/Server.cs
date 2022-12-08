﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BizHawk.Client.EmuHawk.Jotas.Utils
{
	public class Server
	{
		public static string url = "http://localhost:8012/";

		public static async Task HandleIncomingConnections(HttpListener listener, string player, Dictionary<string, Action<dynamic>> routes)
		{
			bool runServer = true;

			while (runServer)
			{
				HttpListenerContext ctx = await listener.GetContextAsync();

				HttpListenerRequest req = ctx.Request;
				HttpListenerResponse resp = ctx.Response;

				Console.WriteLine("Request #: {0}");
				Console.WriteLine(req.Url.ToString());
				Console.WriteLine(req.HttpMethod);
				Console.WriteLine(req.UserHostName);
				Console.WriteLine(req.UserAgent);
				Console.WriteLine();


				if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath.Split('/')[1] == player))
				{
					var command = req.Url.AbsolutePath.Split('/')[2];
					if (routes.ContainsKey(command))
					{
						if(req.HasEntityBody)
						{
							using (Stream body = req.InputStream)
							{
								using (var reader = new StreamReader(body, req.ContentEncoding))
								{
									var text = await reader.ReadToEndAsync();
									Queues.DirectQueue(command, new Task(() =>
									{
										routes[command](text);
									}));
								}
							}
						}
						else
						{
							Queues.DirectQueue(command, new Task(async () =>
							{
								routes[command](null);
							}));
						}
					}
				}

				byte[] data = Encoding.UTF8.GetBytes("");
				resp.ContentType = "text/html";
				resp.ContentEncoding = Encoding.UTF8;
				resp.ContentLength64 = data.LongLength;


				await resp.OutputStream.WriteAsync(data, 0, data.Length);
				resp.Close();
			}
		}

	}
}
