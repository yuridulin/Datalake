using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.Energomera_CE301_ver12
{
	public class Driver : IDriver
	{
		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public event LogEvent LogEvent;

		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfiguration)
		{
			LogEvent("Запуск ...");

			// чтение конфигурации
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfiguration);
			}
			catch (Exception e)
			{
				return Err("Конфигурация не прочитана: " + e.Message + "\n" + e.StackTrace);
			}

			SetBadQuality();
			SetValue("Time", DateTime.Now.ToString("HH:mm:ss"), 192);
			UpdateEvent();

			Worker = new Timer(Configuration.ExchangeDelaySeconds * 1000);
			Worker.Elapsed += (s, e) => Work();

			try
			{
				IsActive = true;

				Worker.Start();

				LogEvent("Мониторинг запущен");

				Task.Run(Work);

				return true;
			}
			catch (Exception e)
			{
				return Err("Неизвестная ошибка: " + e.Message + "\n" + e.StackTrace);
			}
		}

		public void Stop()
		{
			LogEvent("Остановка ...");

			IsActive = false;

			try
			{
				Worker.Stop();
			}
			catch { }

			SetBadQuality(true);
			UpdateEvent();

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			throw new NotImplementedException();
		}

		// Реализация получения данных

		Configuration Configuration { get; set; }

		Stream Stream { get; set; }

		Timer Worker { get; set; }

		bool IsActive { get; set; } = false;

		bool Err(string text)
		{
			LogEvent(text, LogType.ERROR);
			return false;
		}

		void SetBadQuality(bool withTime = false)
		{
			lock (Fields)
			{
				foreach (var v in Fields)
				{
					if (withTime || v.Key != "Time")
					{
						v.Value.Quality = 0;
					}
				}
			}
		}

		void SetValue(string name, object value, ushort quality = 192)
		{
			lock (Fields)
			{
				if (Fields.ContainsKey(name))
				{
					Fields[ name ].Value = value;
					Fields[ name ].Quality = quality;
				}
				else
				{
					Fields.Add(name, new DefField { Value = value, Quality = quality });
				}
			}
		}

		void Work()
		{
			if (!IsActive) return;

			LogEvent("Опрос прибора запущен", LogType.DETAILED);

			try
			{
				var client = new TcpClient();
				client.Connect(Configuration.Endpoint, Configuration.Port);

				Stream = client.GetStream();
				if (!IsActive) return;

				if (Configuration.IsReadingEnergyAndData)
					GetEnergyAndData();
				if (Configuration.IsReadingPowerParams)
					GetPowerParams();

				SetValue("Time", DateTime.Now.ToString("HH:mm:ss"), 192);
				UpdateEvent();
			}
			catch (Exception e)
			{
				LogEvent("Ошибка: " + e.Message, LogType.ERROR);
				SetBadQuality();
				SetValue("Time", DateTime.Now.ToString("HH:mm:ss"), 192);
				UpdateEvent();
			}

			LogEvent("Опрос прибора закончен", LogType.DETAILED);
		}

		byte[] ByteExchange(byte[] tx)
		{
			int read = 0;

			Stream.Write(tx, 0, tx.Length);
			LogEvent("TX: " + Helpers.BytesToString(tx), LogType.DETAILED);

			byte[] rx = new byte[130];
			Task.Run(() => { read = Stream.Read(rx, 0, rx.Length); }).Wait(Configuration.RxTimeoutMs);

			rx = rx.Take(read).ToArray();
			LogEvent("RX: " + (read == 0 ? "Нет ответа" : Helpers.BytesToString(rx)), LogType.DETAILED);

			return rx;
		}

		byte[] SayHello()
		{
			byte[] address = Configuration.DeviceNumber
				.Select(x => (byte)(byte.Parse(x.ToString()) ^ 0x30))
				.ToArray();

			var tx = new List<byte>();
			tx.AddRange(new byte[] { 0x2F, 0x3F });
			tx.AddRange(address);
			tx.AddRange(new byte[] { 0x21, 0x8D, 0x0A });

			return tx
				.Select(x => Configuration.Is7E1 ? EvenModify(x) : x)
				.ToArray();
		}

		byte[] SayPrepare()
		{
			var tx = new List<byte> { 0x06, 0x30, 0x35, 0xB1, 0x8D, 0x0A };

			return tx
				.Select(x => Configuration.Is7E1 ? EvenModify(x) : x)
				.ToArray();
		}

		byte[] SaySmallCommand(string key)
		{
			var tx = new List<byte>();
			tx.AddRange(new byte[] { 0x01, 0x52, 0x31, 0x02 });
			tx.AddRange(Encoding.UTF8.GetBytes(key));
			tx.AddRange(new byte[] { 0x03 });
			tx.Add(BCC(tx));

			return tx
				.Select(x => Configuration.Is7E1 ? EvenModify(x) : x)
				.ToArray();
		}

		void GetPowerParams()
		{
			ByteExchange(SayHello());
			ByteExchange(SayPrepare());

			var commands = new Dictionary<string, string[]>
			{
				{ "VOLTA()", new [] { "VoltageA", "VoltageB", "VoltageC" } },
				{ "CURRE()", new [] { "AmperageA", "AmperageB", "AmperageC" } },
				{ "CORIU()", new [] { "AngleA", "AngleB", "AngleC" } },
				{ "FREQU()", new [] { "Frequency" } },
			};

			foreach (var command in commands)
			{
				if (!IsActive) break;
				var rx = ByteExchange(SaySmallCommand(command.Key));
				if (!IsActive) break;

				if (Configuration.Is7E1) rx = rx.Select(x => EvenRestore(x)).ToArray();

				LogEvent("Получена строка: " + Encoding.UTF8.GetString(rx), LogType.DETAILED);

				var values = Encoding.UTF8.GetString(rx)
					.Split(')')
					.Where(x => x.Contains('('))
					.Select(x => 
					{
						LogEvent("Секция: " + x, LogType.DETAILED);
						LogEvent("[0]: " + x.Split('(')[0], LogType.DETAILED);
						LogEvent("[1]: " + x.Split('(')[1], LogType.DETAILED);
						LogEvent("Значение: " + (float.TryParse(x.Split('(')[1].Replace('.', ','), out float fx) ? fx : 0), LogType.DETAILED);
						return float.TryParse(x.Split('(')[1].Replace('.', ','), out float f) ? f : 0; 
					})
					.ToArray();

				for (int i = 0; i < command.Value.Length; i++)
				{
					LogEvent("Параметр: " + command.Value[i], LogType.DETAILED);
					if (values.Length <= i) break;
					LogEvent("Значение: " + values[i], LogType.DETAILED);
					SetValue(command.Value[i], values[i]);
				}
			}
		}

		void GetEnergyAndData()
		{
			if (!IsActive) return;

			// Составляем запрос
			var command = new List<byte>();

			command.AddRange(new byte[] { 0x2F, 0x3F });
			command.AddRange(Configuration.DeviceNumber
				.Select(x => (byte)(byte.Parse(x.ToString()) ^ 0x30))
				.ToArray());
			command.AddRange(new byte[] { 0x21, 0x01, 0x52, 0x31, 0x02, 0x47, 0x52, 0x4F, 0x55, 0x50, 0x28 });
			command.AddRange(Encoding.UTF8.GetBytes(
				"0001()" +
				"1001(01)" +
				"1301(" + DateTime.Today.AddDays(-1).ToString("ddMMyy") + "01)" +
				"1101(" + DateTime.Today.AddMonths(-1).ToString("MMyy") + "01)"));
			command.AddRange(new byte[] { 0x29, 0x03 });
			command.Add(BCC(command));

			var tx = command
				.Select(x => Configuration.Is7E1 ? EvenModify(x) : x)
				.ToArray();

			// Выполняем байтовый обмен с прибором
			var rx = ByteExchange(tx);
			if (!IsActive) return;

			// Парсим ответ прибора
			if (Configuration.Is7E1) rx = rx.Select(x => EvenRestore(x)).ToArray();

			string answer = Encoding.UTF8.GetString(rx);

			var pairs = answer.Split(')');

			foreach (var pair in pairs)
			{
				if (!pair.Contains('(')) continue;
				var parts = pair.Split('(');
				var val = float.TryParse(parts[1].Replace('.', ','), out float f) ? f : 0;

				if (parts[0] == "1001")
				{
					SetValue("EnergyCurrent", val);
				}
				else if (parts[0] == "1101")
				{
					SetValue("EnergyLastMonth", val);
				}
				else if (parts[0] == "1301")
				{
					SetValue("EnergyLastDay", val);
				}
				else
				{
					string time = parts[1].Substring(2);
					time = "" +
						time[0] + time[1] + "." +
						time[2] + time[3] + ".20" +
						time[4] + time[5] + " " +
						time[6] + time[7] + ":" +
						time[8] + time[9] + ":" +
						time[10] + time[11];

					SetValue("DeviceTime", time);
				}
			}
		}

		byte EvenModify(byte val)
		{
			byte res = val;
			int ev = 0;
			for (int i = 0; i < 8; ++i)
			{
				if ((val & 1) == 1)
					ev++;
				val >>= 1;
			}

			if (ev % 2 == 1)
			{
				if ((res & 0x80) != 0)
					res &= 0x7F;
				else
					res |= 0x80;
			}
			return res;
		}

		byte EvenRestore(byte val)
		{
			return (byte)(val & 0x7F);
		}

		byte BCC(List<byte> data)
		{
			byte bcc = 0;
			bool pass = false;

			for (int i = 0; i < data.Count; i++)
			{
				if (data[i] == 0x03) pass = false;
				if (pass) bcc += data[i];
				if (data[i] == 0x02) pass = true;
			}

			return (byte)(bcc + 8);
		}
	}
}
