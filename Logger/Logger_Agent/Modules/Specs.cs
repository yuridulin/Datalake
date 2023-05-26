using Logger.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Logger.Agent.Modules
{
	public static class Specs
	{
		static List<AgentSpec> Cache { get; set; }

		static bool IsFirstCheck { get; set; } = true;

		static DateTime LastLoad { get; set;} = DateTime.MinValue;

		static Timer Timer { get; set; }

		public static void Start()
		{
			// Рестор данных из локального кэша - отслеживание изменений за время оффлайна
			GetDataFromCache();

			// Удаление возможных прошлых версий отчётов Aida
			try
			{
				if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Additional\AIDA64\report.csv"))
				{
					File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Additional\AIDA64\report.csv");
				}
			}
			catch { }

			// Первый запуск
			Task.Factory.StartNew(() => Timer_Elapsed(null, null));

			// Периодическая проверка изменений
			Timer = new Timer(300000);
			Timer.Elapsed += Timer_Elapsed;
		}

		public static void Stop()
		{
			Timer.Stop();
			Helpers.RaiseEvent(AgentLogSources.Specs, "stopped");
		}

		private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (DateTime.Now - LastLoad >= TimeSpan.FromHours(1))
			{
				LastLoad = DateTime.Now;
				RunAida();
			}

			CheckChanges();
		}

		static void GetDataFromCache()
		{
			try
			{
				string cache = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "specs.json");

				Cache = JsonConvert.DeserializeObject<List<AgentSpec>>(cache);
			}
			catch (Exception ex)
			{
				Helpers.RaiseEvent(AgentLogSources.Specs, "Ошибка при чтении кэша\r\n" + ex.Message + "\r\n" + ex.StackTrace, true);
				Cache = new List<AgentSpec>();
			}
		}

		static void PutDataToCache()
		{
			try
			{
				string cache = JsonConvert.SerializeObject(Cache);
				File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "specs.json", cache);
			}
			catch (Exception ex)
			{
				Helpers.RaiseEvent(AgentLogSources.Specs, "Ошибка при сохранении кэша\r\n" + ex.Message + "\r\n" + ex.StackTrace, true);
			}
		}

		static void RunAida()
		{
			try
			{
				Helpers.RaiseEvent(AgentLogSources.Specs, "Запуск Aida" +
					"\n\tDirectory: " + AppDomain.CurrentDomain.BaseDirectory + @"Additional\AIDA64\" +
					"\n\tFileName: " + AppDomain.CurrentDomain.BaseDirectory + @"Additional\AIDA64\aida64.exe");

				var process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + @"Additional\AIDA64\",
						FileName = AppDomain.CurrentDomain.BaseDirectory + @"Additional\AIDA64\aida64.exe",
						Arguments = "/R \".\\report.csv\" /CUSTOM report.rpf /CSV /SILENT",
						UseShellExecute = false,
						RedirectStandardOutput = true,
					}
				};

				process.Start();
				process.WaitForExit(1000 * 60 * 5);

				Helpers.RaiseEvent(AgentLogSources.Specs, "Отчёт Aida сформирован");
			}
			catch (Exception e)
			{
				Helpers.RaiseEvent(AgentLogSources.Specs, "Ошибка:\r\n" + e.Message + "\r\n" + e.StackTrace, true);
			}
		}

		static void CheckChanges()
		{
			string path = AppDomain.CurrentDomain.BaseDirectory + @"Additional\AIDA64\report.csv";

			// получение данных из созданного отчёта
			var specs = new List<AgentSpec>();

			if (!File.Exists(path))
			{
				Helpers.RaiseEvent(AgentLogSources.Specs, "Не найден файл отчёта Aida по пути " + path, true);
				return;
			}

			using (var stream = new StreamReader(path, Encoding.GetEncoding(1251)))
			{
				string line;
				string[] breaked;
				stream.ReadLine();
				while (!stream.EndOfStream)
				{
					line = stream.ReadLine();
					breaked = line.Split(',');

					if (breaked.Length == 6)
					{
						specs.Add(new AgentSpec
						{
							Page = breaked[ 0 ],
							Device = breaked[ 1 ],
							ItemGroup = breaked[ 2 ],
							ItemId = breaked[ 3 ],
							Item = breaked[ 4 ],
							Value = breaked[ 5 ],
						});
					}
				}
			}
			
			// выполнение сравнения
			bool hasChanges = false;

			if (Cache.Count > 0)
			{
				var buff = new List<AgentSpec>();
				foreach (var spec in Cache.Where(x => x.Page == "Устройства Windows"))
				{
					if (!specs.Where(x => x.Page == "Устройства Windows").Any(x => x.Device + x.ItemGroup + x.ItemId + x.Item + x.Value == spec.Device + spec.ItemGroup + spec.ItemId + spec.Item + spec.Value))
					{
						Helpers.RaiseEvent(AgentLogSources.Specs, "Добавлена спецификация\r\n" + spec.Item + "\r\n" + spec.Value);
						hasChanges = true;
					}
					else
					{
						buff.Add(spec);
					}
				}

				foreach (var spec in specs.Where(x => x.Page == "Устройства Windows"))
				{
					if (!buff.Where(x => x.Page == "Устройства Windows").Any(x => x.Device + x.ItemGroup + x.ItemId + x.Item + x.Value == spec.Device + spec.ItemGroup + spec.ItemId + spec.Item + spec.Value))
					{
						Helpers.RaiseEvent(AgentLogSources.Specs, "Удалена спецификация\r\n" + spec.Item + "\r\n" + spec.Value);
						hasChanges = true;
					}
				}
			}

			// обработка результата
			Cache = specs;
			PutDataToCache();

			if (IsFirstCheck || hasChanges)
			{
				Helpers.RaiseEvent(AgentLogSources.Specs, "Отправка нового списка спецификаций на сервер");
				Sender.AddSpecs(specs);
				IsFirstCheck = false;
			}
		}
	}
}