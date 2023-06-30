using Datalake.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers.Collector
{
	public class CalculatorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					Rebuild();
					Update();
				}
				catch (Exception ex)
				{
					Console.WriteLine(DateTime.Now + " [" + nameof(CalculatorWorker) + "] " + ex.ToString());
				}

				await Task.Delay(1000);
			}
		}

		static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		static List<Tag> Tags { get; set; } = new List<Tag>();

		static void Rebuild()
		{
			using (var db = new DatabaseContext())
			{
				var inputs = db.Rel_Tag_Input.ToList();

				var lastUpdate = db.GetUpdateDate();
				if (lastUpdate == StoredUpdate) return;

				Console.WriteLine("Выполняется пересборка пакетов обновления");

				Tags = db.Tags
					.Where(x => x.IsCalculating)
					.ToList();

				foreach (var tag in Tags)
				{
					tag.Inputs = inputs.Where(x => x.TagId == tag.Id).ToList();
					tag.PrepareToCalc();
				}

				StoredUpdate = lastUpdate;
			}
		}

		static void Update()
		{
			using (var db = new DatabaseContext())
			{
				foreach (var tag in Tags)
				{
					var (text, raw, number, quality) = tag.Calculate();

					db.WriteToHistory(new TagHistory
					{
						TagId = tag.Id,
						Date = DateTime.Now,
						Text = text,
						Raw = raw,
						Number = number,
						Quality = quality,
					});
				}
			}
		}
	}
}
