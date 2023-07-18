using Datalake.Database;
using Datalake.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers.Cache
{
	public static class CacheWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				Rebuild();

				await Task.Delay(1000);
			}
		}

		static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		public static Dictionary<int, TagType> Types { get; set; } = new Dictionary<int, TagType>();

		public static Dictionary<int, TagLive> Live { get; set; } = new Dictionary<int, TagLive>();

		public static void Rebuild()
		{
			using (var db = new DatabaseContext())
			{
				var lastUpdate = db.GetUpdateDate();
				if (lastUpdate == StoredUpdate) return;

				try
				{
					db.Log(nameof(Cache), "Выполняется обновление кэша", ProgramLogType.Warning);

					Types = db.Tags
						.ToDictionary(x => x.Id, x => x.Type);

					StoredUpdate = lastUpdate;

					db.Log(nameof(Cache), "Количество тегов: " + Types.Count, ProgramLogType.Trace);
				}
				catch (Exception ex)
				{
					db.Log(nameof(Cache), ex.Message, ProgramLogType.Error);
				}
				finally
				{
					db.Log(nameof(Cache), "Обновление списка тегов завершено", ProgramLogType.Warning);
				}
			}
		}

		public static object Read(int tagId)
		{
			try
			{
				return Live[tagId].Value(Types[tagId]);
			}
			catch
			{
				return 0;
			}
		}
	}
}
