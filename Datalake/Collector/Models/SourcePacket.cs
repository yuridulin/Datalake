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

			var items = tagsToUpdate
				.Select(x => x.ItemName)
				.Distinct()
				.ToArray();

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

						db.TagsLive
							.Where(x => x.TagName == tag.TagName)
							.Set(x => x.Text, value.GetText())
							.Set(x => x.Number, value.GetNumber())
							.Set(x => x.Quality, (short)value.Quality)
							.Set(x => x.Date, res.Timestamp)
							.Update();

						db.TagsHistory
							.Value(x => x.TagName, tag.TagName)
							.Value(x => x.Text, value.GetText())
							.Value(x => x.Number, value.GetNumber())
							.Value(x => x.Quality, (short)value.Quality)
							.Value(x => x.Date, res.Timestamp)
							.Insert();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Collector error: " + ex.Message);
			}
			finally
			{
				foreach (var tag in tagsToUpdate)
				{
					tag.Reset(now);
				}

				IsActive = false;
			}
		}

		DatalakeResponse AskInopc(string[] tags, string address)
		{
			DatalakeRequest req = new DatalakeRequest
			{
				Tags = tags
			};

			var web = (HttpWebRequest)WebRequest.Create("http://" + address + ":81/api/storage/read");

			web.ContentType = "application/json";
			web.Method = "POST";
			web.Timeout = 5000;

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
