using iNOPC.Library;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ExcelReader
{
	public class Driver : IDriver
	{
		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public event LogEvent LogEvent;

		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfig)
		{
			LogEvent("Запуск ...");

			// чтение конфигурации
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
			}
			catch (Exception e)
			{
				return Err("Конфигурация не прочитана: " + e.Message);
			}

			Fields = new Dictionary<string, DefField>();

			Timer = new Timer(100);
			Timer.Elapsed += (s, e) => LoadData();
			Timer.Start();

			LogEvent("Мониторинг запущен");

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
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}


		// Реализация получения данных

		Configuration Configuration { get; set; }

		Timer Timer { get; set; }

		bool LoadData()
		{
			if (Timer.Interval == 100) Timer.Interval = Math.Max(1000, Configuration.MinutesReadInterval * 60 * 1000);

			if (!File.Exists(Configuration.FilePath)) return Err("Файл не найден");

			string type = Configuration.FilePath.Substring(Configuration.FilePath.LastIndexOf(".") + 1);
			if (!new string[] {"xls", "xlsx" }.Contains(type)) return Err("Тип файла не поддерживается");

			try
			{
				// Открытие существующей рабочей книги
				IWorkbook workbook;
				using (var fileStream = new FileStream(Configuration.FilePath, FileMode.Open, FileAccess.Read))
				{
					if (type == "xlsx")
					{
						workbook = new XSSFWorkbook(fileStream);
					}
					else
					{
						workbook = new HSSFWorkbook(fileStream);
					}
				}

				IFormulaEvaluator eval;
				if (workbook is XSSFWorkbook)
					eval = new XSSFFormulaEvaluator(workbook);
				else
					eval = new HSSFFormulaEvaluator(workbook);


				var values = Configuration.Cells.Select(x => new DefField { Name = x.Name, Quality = 0 }).ToDictionary(x => x.Name, x => x);

				foreach (var sheetValues in Configuration.Cells.Where(x => x.IsActive).GroupBy(x => x.Sheet))
				{
					// Получение листа
					ISheet sheet = workbook.GetSheetAt((int)sheetValues.Key);

					// Чтение данных из ячейки
					foreach (var info in sheetValues.Where(x => x.IsActive))
					{
						try
						{
							var row = sheet.GetRow((int)info.Row);
							var cell = row.GetCell((int)info.Column);
							object value = cell.GetFormattedCellValue(eval);

							values[info.Name].Value = value;
							values[info.Name].Quality = 192;
						}
						catch (Exception ex)
						{
							Err("Не получено значение \"" + info.Name + "\": " + ex.Message);
						}
					}
				}

				lock (Fields)
				{
					foreach (var v in values)
					{
						Fields[v.Key] = v.Value;
					}
					Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
				}
				
				UpdateEvent();
				return true;
			}
			catch (Exception ex)
			{
				lock (Fields)
				{
					Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
				}

				UpdateEvent();
				return Err(ex.Message + "\n" + ex.StackTrace);
			}
		}

		bool Err(string message)
		{
			LogEvent(message, LogType.ERROR);
			return false;
		}
	}
}
