using Datalake.Database;
using Datalake.Database.Enums;
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
			using (var db = new DatabaseContext())
			{
				var lastUpdate = db.GetUpdateDate();
				if (lastUpdate == StoredUpdate) return;

				try
				{
					db.Log("Calc", "Выполняется обновление списка тегов", ProgramLogType.Warning);

					var inputs = db.Rel_Tag_Input.ToList();

					Tags = db.Tags
						.Where(x => x.IsCalculating)
						.ToList();

					db.Log("Calc", "Количество вычисляемых тегов: " + Tags.Count, ProgramLogType.Trace);

					foreach (var tag in Tags)
					{
						tag.Inputs = inputs.Where(x => x.TagId == tag.Id).ToList();
						tag.PrepareToCalc();
					}

					StoredUpdate = lastUpdate;
				}
				catch (Exception ex)
				{
					db.Log("Calc", ex.Message, ProgramLogType.Error);
				}
				finally
				{
					db.Log("Calc", "Обновление списка тегов завершено", ProgramLogType.Warning);
				}
			}
		}

		static void Update()
		{
			using (var db = new DatabaseContext())
			{
				try
				{
					db.Log("Calc", "Выполняется расчёт значений", ProgramLogType.Trace);

					foreach (var tag in Tags)
					{
						db.Log("Calc", "Расчёт тега [" + tag.Name + "]", ProgramLogType.Trace);

						var (text, raw, number, quality) = tag.Calculate();

						db.Log("Calc", "Новое значение тега [" + tag.Name + "] = RAW:" + raw, ProgramLogType.Trace);

						db.WriteToHistory(new TagHistory
						{
							TagId = tag.Id,
							Date = DateTime.Now,
							Text = text,
							Raw = raw,
							Number = number,
							Quality = quality,
						});

						db.Log("Calc", "Значение тега [" + tag.Name + "] сохранено в базе", ProgramLogType.Trace);
					}
				}
				catch (Exception ex)
				{
					db.Log("Calc", ex.Message, ProgramLogType.Error);
				}
				finally
				{
					db.Log("Calc", "Расчёт значений завершен", ProgramLogType.Trace);
				}
			}
		}
	}
}
