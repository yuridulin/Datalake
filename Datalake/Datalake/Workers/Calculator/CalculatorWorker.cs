using Datalake.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers.Calculator
{
	public class CalculatorWorker
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
				try
				{
					db.Log("Calc", "Выполняется обновление списка тегов", Database.Enums.ProgramLogType.Warning);

					var inputs = db.Rel_Tag_Input.ToList();

					var lastUpdate = db.GetUpdateDate();
					if (lastUpdate == StoredUpdate) return;

					Tags = db.Tags
						.Where(x => x.IsCalculating)
						.ToList();

					db.Log("Calc", "Количество вычисляемых тегов: " + Tags.Count, Database.Enums.ProgramLogType.Trace);

					foreach (var tag in Tags)
					{
						tag.Inputs = inputs.Where(x => x.TagId == tag.Id).ToList();
						tag.PrepareToCalc();
					}

					StoredUpdate = lastUpdate;
				}
				catch (Exception ex)
				{
					db.Log("Calc", ex.Message, Database.Enums.ProgramLogType.Error);
				}
				finally
				{
					db.Log("Calc", "Обновление списка тегов завершено", Database.Enums.ProgramLogType.Warning);
				}
			}
		}

		static void Update()
		{
			using (var db = new DatabaseContext())
			{
				try
				{
					db.Log("Calc", "Выполняется расчёт значений", Database.Enums.ProgramLogType.Trace);

					foreach (var tag in Tags)
					{
						db.Log("Calc", "Расчёт тега [" + tag + "]", Database.Enums.ProgramLogType.Trace);

						var (text, raw, number, quality) = tag.Calculate();

						db.Log("Calc", "Новое значение тега [" + tag + "] = RAW:" + raw, Database.Enums.ProgramLogType.Trace);

						db.WriteToHistory(new TagHistory
						{
							TagId = tag.Id,
							Date = DateTime.Now,
							Text = text,
							Raw = raw,
							Number = number,
							Quality = quality,
						});

						db.Log("Calc", "Значение тега [" + tag + "] сохранено в базе", Database.Enums.ProgramLogType.Trace);
					}
				}
				catch (Exception ex)
				{
					db.Log("Calc", ex.Message, Database.Enums.ProgramLogType.Error);
				}
				finally
				{
					db.Log("Calc", "Расчёт значений завершен", Database.Enums.ProgramLogType.Trace);
				}
			}
		}
	}
}
