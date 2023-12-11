using Datalake.Database;
using Datalake.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers
{
	public static class CollectorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				using (var db = new DatabaseContext())
				{
					try
					{
						if (Cache.LastUpdate > StoredUpdate)
						{
							Sources = db.Sources.ToList();
							Sources.ForEach(source => source.Rebuild(db));
							StoredUpdate = Cache.LastUpdate;
						}

						Sources.ForEach(source => source.Update(db));
					}
					catch (Exception ex)
					{
						db.Log(new Log
						{
							Category = LogCategory.Collector,
							Type = LogType.Error,
							Text = "Ошибка в цикле обновления",
							Exception = ex,
						});
					}
				}
				
				await Task.Delay(1000);
			}
		}

		static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		static List<Source> Sources { get; set; } = new List<Source>();
	}
}
