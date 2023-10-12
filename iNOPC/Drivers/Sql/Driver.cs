using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Timers;

namespace Sql
{

	public class Driver : IDriver
	{
		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public event LogEvent LogEvent;

		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfig)
		{
			LogEvent("Запуск...");

			Fields = new Dictionary<string, DefField>();

			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
			}
			catch (Exception e)
			{
				LogEvent("Конфигурация не прочитана: " + e.Message, LogType.ERROR);
				return false;
			}

			Timer = new Timer(Configuration.IntervalInSeconds * 1000);
			Timer.Elapsed += (s, e) => Update();
			Timer.Start();

			Fields["Connection"] = new DefField { Value = true, Quality = 192 };
			Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
			UpdateEvent();

			LogEvent("Мониторинг активен");

			return true;
		}

		public void Stop()
		{
			LogEvent("Остановка...");

			Fields["Connection"] = new DefField { Value = false, Quality = 192 };

			try
			{
				Timer?.Stop();
				Timer = null;
			}
			catch { }

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			Fields[fieldName].Value = value;
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}

		Configuration Configuration { get; set; } = new Configuration();

		Timer Timer { get; set; }

		void Update()
		{
			LogEvent("Попытка выполнить запрос");

			try
			{
				using (var conn = new OdbcConnection())
				{
					conn.ConnectionString = "Driver={SQL Server}; " +
						"Server=" + Configuration.ServerName + "; " +
						"Database=" + Configuration.DatabaseName + "; " +
						"Uid=" + Configuration.Username + "; " +
						"Pwd=" + Configuration.Password + ";";
					conn.Open();

					using (var command = new OdbcCommand())
					{
						command.Connection = conn;
						command.CommandText = Configuration.Code;

						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								var key = reader.GetString(0);
								var value = reader.GetValue(1);

								Value(key, value);
							}
						}
					}

					conn.Close();
				}

				Value("Time", DateTime.Now.ToString("HH:mm:ss"));
				LogEvent("Запрос выполнен");
				UpdateEvent();
			}
			catch (Exception ex) 
			{
				LogEvent("Ошибка: " + ex.Message, LogType.ERROR);
			}

		}

		void Value(string key, object value, ushort quality = 192)
		{
			if (Fields.ContainsKey(key))
			{
				Fields[key].Value = value;
				Fields[key].Quality = quality;
			}
			else
			{
				Fields.Add(key, new DefField { Name = key, Value = value, Quality = quality });
			}
		}
	}
}
