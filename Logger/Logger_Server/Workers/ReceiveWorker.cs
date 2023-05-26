using LinqToDB;
using LinqToDB.Data;
using Logger.Database;
using Logger.Library;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.Workers
{
	public static class ReceiveWorker
	{
		public static async Task Work(CancellationToken token)
		{
			var listener = new HttpListener();
			listener.Prefixes.Add("http://*:4330/");
			listener.IgnoreWriteExceptions = true;
			listener.Start();

			while (!token.IsCancellationRequested)
			{
				var ctx = await listener.GetContextAsync();
				
				try 
				{
					string json = new StreamReader(ctx.Request.InputStream).ReadToEnd();

					var reply = JsonConvert.DeserializeObject<AgentReply>(json);
					var config = await Answer(reply);

					json = JsonConvert.SerializeObject(config);

					ctx.Response.StatusCode = 200;
					ctx.Response.ContentType = "application/json";
					using (var writer = new StreamWriter(ctx.Response.OutputStream))
					{
						writer.WriteLine(json);
					}
				}
				catch (Exception e)
				{
					Helpers.RaiseServerEvent(ServerLogSources.Receive, e.Message, true);
				}
			}
		}

		static async Task<AgentConfig> Answer(AgentReply reply)
		{
			using (var db = new DatabaseContext())
			{
				// определение станции
				if (!db.Stations.Any(x => x.Endpoint == reply.Endpoint))
				{
					Helpers.RaiseServerEvent(ServerLogSources.Receive, reply.LastUpdate + " | " + reply.Endpoint + " " + reply.Logs.Count + "L " + reply.Specs.Count + "S " + " unknown station", true);
					return new AgentConfig { LastUpdate = reply.LastUpdate };
				}
				else
				{
					await db.Stations
						.Where(x => x.Endpoint.ToUpper() == reply.Endpoint.ToUpper())
						.Set(x => x.LastTimeAlive, DateTime.Now)
						.Set(x => x.AgentVersion, reply.Version)
						.UpdateAsync();
				}

				// получение данных
				if (reply.Logs.Count > 0)
				{
					await db.BulkCopyAsync(reply.Logs.Select(x => new Log
					{
						Category = x.Category,
						Checked = false,
						Endpoint = reply.Endpoint,
						EventId = x.EventId,
						Journal = x.Journal,
						LogFilterId = x.LogFilterId,
						Message = x.Message,
						Source = x.Source,
						TimeStamp = x.TimeStamp,
						Type = x.Type,
						Username = x.Username,
					}));
				}

				if (reply.Specs.Count > 0)
				{
					await db.StationsSpecs
						.DeleteAsync(x => x.Endpoint.ToUpper() == reply.Endpoint.ToUpper());

					await db.BulkCopyAsync(reply.Specs.Select(x => new StationSpec
					{
						Endpoint = reply.Endpoint,
						Page = x.Page,
						Device = x.Device,
						ItemGroup = x.ItemGroup,
						ItemId = x.ItemId,
						Item = x.Item,
						Value = x.Value,
					}));
				}

				// Ответ
				var lastUpdate = await db.Settings
					.Select(x => x.LastUpdate)
					.DefaultIfEmpty(DateTime.MinValue)
					.FirstOrDefaultAsync();

				if (CacheWorker.LastUpdate > reply.LastUpdate)
				{
					var configId = await db.Stations
						.Where(x => x.Endpoint.ToUpper() == reply.Endpoint.ToUpper())
						.Select(x => x.StationConfigId)
						.FirstAsync();

					// Обращение к кэшу конфигов за актуальной версией нужного конфига
					if (CacheWorker.CachedConfigs.TryGetValue(configId, out AgentConfig config))
					{
						Helpers.RaiseServerEvent(ServerLogSources.Receive, reply.LastUpdate + " | " + reply.Endpoint + " " + reply.Logs.Count + "L " + reply.Specs.Count + "S " + " update << "
							+ config.LastUpdate + " [" + configId + "] " + config.Filters.Count + "F " + config.Pings.Count + "P " + config.SqlActions.Count + "S");
						return config;
					}
					else
					{
						Helpers.RaiseServerEvent(ServerLogSources.Receive, reply.LastUpdate + " | " + reply.Endpoint + " " + reply.Logs.Count + "L " + reply.Specs.Count + "S " + " update, but not exist in cache [" + configId + "]", true);
						return new AgentConfig { LastUpdate = reply.LastUpdate };
					}
				}
				else
				{
					Helpers.RaiseServerEvent(ServerLogSources.Receive, reply.LastUpdate + " | " + reply.Endpoint + " " + reply.Logs.Count + "L " + reply.Specs.Count + "S " + " actual");
					return new AgentConfig { LastUpdate = reply.LastUpdate };
				}
			}
		}
	}
}
