using iNOPC.Drivers.PowerCounters.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.PowerCounters
{
	public class Driver : IDriver
	{
		public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public string Version => typeof(Driver).Assembly.GetName().Version.ToString();

		public event LogEvent LogEvent;
		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfiguration)
		{
			LogEvent("Запуск ...");
			Fields.Clear();
			UpdateValue("Time", DateTime.Now.ToString("HH:mm:ss"));

			// чтение конфигурации
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfiguration);
			}
			catch (Exception e)
			{
				LogEvent("Конфигурация не прочитана: " + e.Message, LogType.ERROR);
				return false;
			}

			Timer = new Timer(Configuration.CyclicTimeout * 60 * 1000);
			Timer.Elapsed += (s, e) => LoadData();
			Timer.Start();

			LogEvent("Мониторинг запущен");

			Task.Run(() => LoadData());

			return true;
		}

		public void Stop()
		{
			LogEvent("Остановка ...");

			Timer.Stop();

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			// Определение типа записываемого поля
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}


		// Реализация получения данных

		Configuration Configuration { get; set; }

		Timer Timer { get; set; }

		void LoadData()
		{
			LogEvent("Срабатывание таймера в " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogType.DETAILED);
			Dictionary<string, object> values = new Dictionary<string, object>();

			try
			{
				string url = Configuration.Link + "/?date=" + DateTime.Today.ToString("yyyy-MM-dd");
				string raw = GetHtml(url);

				try
				{
					Dictionary<int, Entry> entries = JsonConvert.DeserializeObject<Dictionary<int, Entry>>(raw);

					foreach (var pair in entries)
					{
						values.Add(pair.Key + ".Date", pair.Value.date);
						values.Add(pair.Key + ".Name", pair.Value.name);
						values.Add(pair.Key + ".Nominal", pair.Value.nominal);
						values.Add(pair.Key + ".Power", pair.Value.power);
					}
				}
				catch (Exception e)
				{
					LogEvent("Ошибка при преобразовании в json [" + e.Message + "]", LogType.ERROR);
				}
			}
			catch (Exception e)
			{
				LogEvent("Ошибка при получении данных [" + e.Message + "]", LogType.ERROR);
			}

			try
			{
				lock (Fields)
				{
					foreach (var pair in values) UpdateValue(pair.Key, pair.Value);
					UpdateValue("Time", DateTime.Now.ToString("HH:mm:ss"));
				}
			}
			catch (Exception e)
			{
				LogEvent("Ошибка при обновлении данных [" + e.Message + "]", LogType.ERROR);
			}

			UpdateEvent();
		}

		string GetHtml(string url)
		{
			try
			{
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
				req.Method = "GET";
				req.Timeout = 10000;
				req.Credentials = new NetworkCredential("modbus", "1");

				WebResponse res = req.GetResponse();
				StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);

				string raw = sr.ReadToEnd();

				sr.Close();
				res.Close();

				return raw;
			}
			catch (Exception e)
			{
				LogEvent("Ошибка при HTTP запросе к источнику данных: " + e.Message, LogType.ERROR);
				return null;
			}
		}

		void UpdateValue(string name, object value)
		{
			try
			{
				if (Fields.ContainsKey(name))
				{
					Fields[name] = new DefField
					{
						Name = name,
						Quality = 192,
						Value = value,
					};
				}
				else
				{
					Fields.Add(name, new DefField
					{

						Name = name,
						Quality = 192,
						Value = value,
					});
				}
			}
			catch
			{

			}
		}
	}
}
