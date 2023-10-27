using Datalake.Web.Models;
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
	public static class Collector
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
				web.Timeout = 2000;

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
				LogsWorker.Add("Inopc", "Запрос к " + address + ": " + ex.Message, LogType.Error);
				res = new DatalakeResponse
				{
					Timestamp = DateTime.Now,
					Tags = new InputTag[0]
				};
			}

			return res;
		}

		public static DatalakeResponse AskDatalake(string[] tags, string address)
		{
			var res = new List<HistoryResponse>();

			try
			{
				var web = (HttpWebRequest)WebRequest.Create("http://" + address + ":83/api/tags/live");

				web.ContentType = "application/json";
				web.Method = "POST";
				web.Timeout = 2000;

				using (var stream = web.GetRequestStream())
				{
					using (var streamWriter = new StreamWriter(stream))
					{
						string json = JsonConvert.SerializeObject(new { tags });
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
							res = JsonConvert.DeserializeObject<List<HistoryResponse>>(json);
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogsWorker.Add("DatalakeNode", "Запрос к " + address + ": " + ex.Message, LogType.Error);
			}

			var inputs = new List<InputTag>();
			foreach (var tag in res)
			{
				inputs.AddRange(tag.Values.Select(x => new InputTag
				{
					Name = tag.TagName,
					Quality = (ushort)x.Quality,
					Value = x.Value,
					Type = tag.Type,
				}));
			}

			return new DatalakeResponse
			{
				Timestamp = DateTime.Now,
				Tags = inputs.ToArray()
			};
		}
	}
}
