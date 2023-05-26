using LinqToDB;
using Logger.Database;
using Logger.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.Workers
{
	public static class CacheWorker
	{
		public static Dictionary<int, AgentConfig> CachedConfigs { get; set; } = new Dictionary<int, AgentConfig>();

		public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

		public static async Task Work(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				//Helpers.RaiseServerEvent(ServerLogSources.Cache, "Check cache");

				using (var db = new DatabaseContext())
				{
					// проверка, не изменилась ли конфигурация
					var currentUpdate = await db.Settings
						.Select(x => x.LastUpdate)
						.DefaultIfEmpty(DateTime.MinValue)
						.FirstOrDefaultAsync();

					if (currentUpdate > LastUpdate)
					{
						var newCache = new Dictionary<int, AgentConfig>();
						var time = DateTime.Now;

						var configs = await db.StationsConfigs
							.Select(x => x.Id)
							.ToListAsync();

						foreach (var id in configs)
						{
							int i;
							var filtersQuery = from r in db.Rel_StationConfig_LogFilter
											   from x in db.LogsFilters.InnerJoin(x => x.Id == r.LogFilterId)
											   where r.StationConfigId == id
											   select new AgentLogFilter
											   {
												   Id = x.Id,
												   Allow = x.Allow,
												   Categories = x.Categories.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
												   Endpoints = x.Endpoints.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
												   EventIds = x.EventIds.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(k => int.TryParse(k, out i) ? i : 0).ToArray(),
												   Journals = x.Journals.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
												   Sources = x.Sources.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
												   Types = x.Types.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
											   };

							var filters = await filtersQuery.ToListAsync();

							var pingsQuery = from r in db.Rel_StationConfig_ActionPing
											 from x in db.ActionsPings.InnerJoin(x => x.Id == r.PingActionId)
											 where r.StationConfigId == id
											 select new AgentActionPing
											 {
												 Target = x.Target,
												 Interval = x.Interval,
												 Template = x.Template,
												 Value = x.Value,
											 };

							var pings = await pingsQuery.ToListAsync();

							var sqlQuery = from r in db.Rel_StationConfig_ActionSql
										   from x in db.ActionsSql.InnerJoin(x => x.Id == r.ActionSqlId)
										   where r.StationConfigId == id
										   select x;

							var sql = await sqlQuery.ToListAsync();

							var sqlActions = sql
								.Select(x => new AgentActionSql
								{
									CommandCode = x.CommandCode,
									CommandTimeout = x.CommandTimeout,
									Comparers = x.Comparers(),
									ConnectionString = x.ConnectionString,
									DatabaseType = x.DatabaseType,
									Interval = x.Interval,
								})
								.ToList();

							Console.WriteLine("Config [" + id + "] : " + filters.Count + "F " + pings.Count + "P " + sqlActions.Count + "S");
							newCache.Add(id, new AgentConfig
							{
								LastUpdate = currentUpdate,
								Filters = filters,
								Pings = pings,
								SqlActions = sqlActions
							});
						}

						CachedConfigs = newCache;
						LastUpdate = currentUpdate;

						Helpers.RaiseServerEvent(ServerLogSources.Cache, "Cache updated: " + LastUpdate);
					}
				}

				// ожидание следующего подхода
				Task.Delay(TimeSpan.FromSeconds(5)).Wait();
			}
		}
	}
}
