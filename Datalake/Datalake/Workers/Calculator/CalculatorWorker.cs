using Datalake.Database;
using Datalake.Database.Enums;
using Datalake.Workers.Logs;
using Datalake.Workers.Logs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers.Calculator
{
	public static class CalculatorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				Rebuild();
				Update();

				await Task.Delay(1000);
			}
		}

		static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		static List<Tag> Tags { get; set; } = new List<Tag>();

		static void Rebuild()
		{
			if (Cache.LastUpdate == StoredUpdate) return;

			using (var db = new DatabaseContext())
			{
				var inputs = db.Rel_Tag_Input.ToList();

				Tags = db.Tags
					.Where(x => x.IsCalculating)
					.ToList();

				foreach (var tag in Tags)
				{
					tag.Inputs = inputs.Where(x => x.TagId == tag.Id).ToList();
					tag.PrepareToCalc();
				}

				StoredUpdate = Cache.LastUpdate;
			}
		}

		static void Update()
		{
			var values = new List<TagHistory>();

			foreach (var tag in Tags)
			{
				var (text, number, quality) = tag.Calculate();

				var value = new TagHistory
				{
					TagId = tag.Id,
					Date = DateTime.Now,
					Text = text,
					Number = number,
					Quality = quality,
					Type = tag.Type,
					Using = TagHistoryUse.Basic,
				};

				if (Cache.IsNew(value)) values.Add(value);
			}

			if (values.Count > 0)
			{
				using (var db = new DatabaseContext())
				{
					db.WriteHistory(values);
				}
			}

			LogsWorker.Add("Calc", "Вычислено новых значений: " + values.Count, LogType.Trace);
		}
	}
}
