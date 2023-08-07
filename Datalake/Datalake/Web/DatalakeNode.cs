using Datalake.Database;
using Datalake.Workers.Collector.Models;
using Datalake.Workers.Logs;
using Datalake.Workers.Logs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Datalake.Web
{
	public static class DatalakeNode
	{
		public static List<string> AskNode(string[] tags, string address)
		{
			DatalakeRequest req = new DatalakeRequest
			{
				Tags = tags
			};

			List<Tag> res;

			try
			{
				var web = (HttpWebRequest)WebRequest.Create("http://" + address + ":4330/api/tags/list");

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
							res = JsonConvert.DeserializeObject<List<Tag>>(json);
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogsWorker.Add("DatalakeNode", "Запрос к " + address + ": " + ex.Message, LogType.Error);
				res = new List<Tag>();
			}

			return res.Select(x => x.Name).ToList();
		}
	}
}
