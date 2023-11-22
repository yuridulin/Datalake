using Datalake.Database;
using Datalake.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers
{
	public static class CalculatorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			using (var db = new DatabaseContext())
			{
				while (!token.IsCancellationRequested)
				{
					try
					{
						if (Cache.LastUpdate > StoredUpdate)
						{
							var inputs = db.Rel_Tag_Input.ToList();

							Tags = db.Tags
								.Where(x => x.SourceId == CustomSourcesIdentity.Calculated)
								.ToList();

							foreach (var tag in Tags)
							{
								tag.Inputs = inputs.Where(x => x.TagId == tag.Id).ToList();
								tag.PrepareToCalc();
							}

							StoredUpdate = Cache.LastUpdate;
						}

						Update(db);
					}
					catch (Exception ex)
					{
						LogsWorker.Add("Calculator", "Loop error: " + ex.Message, LogType.Error);
					}

					await Task.Delay(1000);
				}
			}
		}

		static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		static List<Tag> Tags { get; set; } = new List<Tag>();

		static void Update(DatabaseContext db)
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
				db.WriteHistory(values);
				LogsWorker.Add("Calc", "Вычислено значений: " + values.Count, LogType.Trace);
			}
		}
	}
}
