using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers
{
	public class CollectorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					Work();
				}
				catch (Exception ex)
				{
					Console.WriteLine(DateTime.Now + " [" + nameof(CollectorWorker) + "] " + ex.ToString());
				}

				await Task.Delay(1000);
			}
		}

		public static void Work()
		{
			//using (var db = new DatabaseContext())
			//{
			//	var tags = db.Tags
			//		.ToList();

			//	Console.WriteLine(tags.Count);
			//}

			var model = new
			{
				Address = "10.178.9.91",
				Tags = new[] { "TestTag", "Ntp1.Time" }
			};

			var req = (HttpWebRequest)WebRequest.Create("http://" + model.Address + ":81/api/storage/read");

			req.ContentType = "application/json";
			req.Method = "POST";
			req.Timeout = 5000;

			string json = JsonConvert.SerializeObject(new { model.Tags });

			using (var streamWriter = new StreamWriter(req.GetRequestStream()))
			{
				streamWriter.Write(json);
			}

			string text;
			var res = (HttpWebResponse)req.GetResponse();
			using (var streamReader = new StreamReader(res.GetResponseStream()))
			{
				text = streamReader.ReadToEnd();
			}

			Console.WriteLine(text);
		}
	}
}
