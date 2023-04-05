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
				.Select(x => x.TagName)
				.ToArray();

			try
			{
				IsActive = true;

				var res = AskInopc(tagsToUpdate, Address);

				using (var db = new DatabaseContext())
				{
					foreach (var item in res.Tags)
					{
						db.TagsLive
							.Where(x => x.TagName == item.Name)
							.Set(x => x.Text, item.GetText())
							.Set(x => x.Number, item.GetNumber())
							.Set(x => x.Quality, (short)item.Quality)
							.Set(x => x.Date, res.Timestamp)
							.Update();

						db.TagsHistory
							.Value(x => x.TagName, item.Name)
							.Value(x => x.Text, item.GetText())
							.Value(x => x.Number, item.GetNumber())
							.Value(x => x.Quality, (short)item.Quality)
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
				foreach (var tag in Tags.Where(x => tagsToUpdate.Contains(x.TagName)))
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
