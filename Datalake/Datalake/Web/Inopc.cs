using Datalake.Workers.Collector.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace Datalake.Web
{
	public static class Inopc
	{
		public static DatalakeResponse AskInopc(string[] tags, string address)
		{
			DatalakeRequest req = new DatalakeRequest
			{
				Tags = tags
			};

			DatalakeResponse res;

			try
			{
				var web = (HttpWebRequest)WebRequest.Create("http://" + address + ":81/api/storage/read");

				web.ContentType = "application/json";
				web.Method = "POST";
				web.Timeout = 1000;

				using (var stream = web.GetRequestStream())
				{
					using (var streamWriter = new StreamWriter(stream))
					{
						string json = JsonConvert.SerializeObject(req);
						streamWriter.Write(json);
					}
				}

				using (var response = (HttpWebResponse)web.GetResponse())
				{
					using (var stream = response.GetResponseStream())
					{
						using (var streamReader = new StreamReader(stream))
						{
							string json = streamReader.ReadToEnd();
							res = JsonConvert.DeserializeObject<DatalakeResponse>(json);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(DateTime.Now + " [" + nameof(Inopc) + "] " + ex.ToString());
				res = new DatalakeResponse
				{
					Timestamp = DateTime.Now,
					Tags = new InopcTag[0]
				};
			}

			return res;
		}
	}
}
