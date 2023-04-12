using Datalake.Database;
using LinqToDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Datalake.Collector.Models
{
	public class SourcePacket
	{
		public string Address { get; set; }

		public List<SourceItem> Tags { get; set; } = new List<SourceItem>();

		bool IsActive { get; set; } = false;

		public void Update()
		{
			if (IsActive) return;

			var now = DateTime.Now;
			var tagsToUpdate = Tags
				.Where(x => x.IsTimed(now))
				.ToList();

			if (tagsToUpdate.Count == 0) return;

			var items = tagsToUpdate
				.Select(x => x.ItemName)
				.Distinct()
				.ToArray();

			Console.WriteLine("Обновление тегов: " + string.Join(", ", tagsToUpdate.Select(x => x.TagName)));

			try
			{
				IsActive = true;

				var res = AskInopc(items, Address);

				using (var db = new DatabaseContext())
				{
					foreach (var tag in tagsToUpdate)
					{
						var value = res.Tags.FirstOrDefault(x => x.Name == tag.ItemName);
						if (value == null)
						{
							Console.WriteLine($"Не найден тег по адресу: {tag.ItemName}");
							continue;
						}

						Console.WriteLine($"Запись тега {tag.TagName}: {value.Value}");

						db.WriteToHistory(tag.TagName, res.Timestamp, value.GetText(), value.GetNumber(), (short)value.Quality);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(DateTime.Now + " [" + nameof(CollectorWorker) + "] " + ex.ToString());
			}
			finally
			{
				var tagNames = tagsToUpdate.Select(x => x.TagName).ToList();
				foreach (var tag in Tags)
				{
					if (tagNames.Contains(tag.TagName)) tag.Reset(now);
				}

				IsActive = false;
			}
		}

		public static DatalakeResponse AskInopc(string[] tags, string address)
		{
			DatalakeRequest req = new DatalakeRequest
			{
				Tags = tags
			};

			var web = (HttpWebRequest)WebRequest.Create("http://" + address + ":81/api/storage/read");

			web.ContentType = "application/json";
			web.Method = "POST";
			web.Timeout = 1000;

			using (var streamWriter = new StreamWriter(web.GetRequestStream()))
			{
				string json = JsonConvert.SerializeObject(req);
				streamWriter.Write(json);
			}

			DatalakeResponse res;
			var webRes = (HttpWebResponse)web.GetResponse();
			using (var streamReader = new StreamReader(webRes.GetResponseStream()))
			{
				string json = streamReader.ReadToEnd();
				res = JsonConvert.DeserializeObject<DatalakeResponse>(json);
			}

			return res;
		}
	}
}
