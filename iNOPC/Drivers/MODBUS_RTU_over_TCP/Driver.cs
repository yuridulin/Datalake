using iNOPC.Library;
using MODBUS_RTU_over_TCP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MODBUS_RTU_over_TCP
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
				return Err("Конфигурация не прочитана: " + e.Message + "\n" + e.StackTrace);
			}

			if (Configuration.Fields.Count == 0)
			{
				return Err("Список опрашиваемых полей пуст");
			}
			else if (!Fields.ContainsKey("Time"))
			{
				Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 });
			}

			UpdateEvent();

			try
			{
				Packages.Clear();
				CreatePackages();
			}
			catch (Exception e)
			{
				return Err("Ошибка при обработке полей: " + e.Message + "\n" + e.StackTrace);
			}

			try
			{
				Active = true;
				ErrCount = 0;

				Thread = new Thread(() =>
				{
					while (Active) Monitoring();
				});
				Thread.Start();

				LogEvent("Мониторинг запущен");

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

			Active = false;

			try { Stream?.Close(); } catch (Exception) { }
			try { Stream = null; } catch (Exception) { }
			try { Client?.Close(); } catch (Exception) { }
			try { Client = null; } catch (Exception) { }
			try { Thread?.Abort(); } catch (Exception) { }
			try { Thread = null; } catch (Exception) { }

			foreach (var field in Fields)
			{
				if (field.Key != "Time") field.Value.Quality = 0;
			}
			UpdateEvent();

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}


		// Реализация получения данных

		Configuration Configuration { get; set; } = null;

		TcpClient Client { get; set; }

		NetworkStream Stream { get; set; }

		//TcpListener Listener { get; set; }

		Thread Thread { get; set; }

		//Thread Thread2 { get; set; }

		List<Package> Packages { get; set; } = new List<Package>();

		DateTime RequestStart { get; set; }

		bool Active { get; set; }

		int ErrCount { get; set; } = 0;

		void CreatePackages()
		{
			// подготовка полей к опросу
			// формирование пакетов, по которым будут запрашиваться данные
			Packages = new List<Package>();

			foreach (var field in Configuration.Fields)
			{
				field.Checked = false;

				if (field.HexAddress != "0x0000")
				{
					field.Address = ushort.Parse(field.HexAddress.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
				}
			}

			ushort i = 0;

			if (Configuration.Multicast)
			{
				Configuration.Fields = Configuration.Fields.OrderBy(x => x.Address).ToList();

				while (Configuration.Fields.Count(x => !x.Checked) > 0)
				{
					var fields = Configuration.Fields.Where(x => !x.Checked).ToArray();

					Package package = null;
					byte length = 0;

					for (int k = 0; k < Math.Min(fields.Length, 61); k++)
					{
						if (k == 0 || (fields[k].Address == fields[k - 1].Address + length))
						{
							length = GetRegistersCount(fields[k].Type);

							if (package == null)
							{
								package = new Package
								{
									CommandCode = fields[0].CommandCode,
									StartAddress = fields[0].Address,

									SlaveId = Configuration.SlaveId,
									OldByteFirst = Configuration.OldByteFirst,
									OldRegisterFirst = Configuration.OldRegisterFirst,
								};
							}

							package.Parts.Add(new PackagePart
							{
								FieldName = fields[k].Name,
								Length = length,
								Type = fields[k].Type,
							});

							fields[k].Checked = true;
						}
						else { break; }
					}

					Packages.Add(package);
					i++;
				}
			}
			else
			{
				foreach (var field in Configuration.Fields.Where(x => !x.Checked))
				{
					Packages.Add(new Package
					{
						CommandCode = field.CommandCode,
						StartAddress = field.Address,

						SlaveId = Configuration.SlaveId,
						OldByteFirst = Configuration.OldByteFirst,
						OldRegisterFirst = Configuration.OldRegisterFirst,

						Parts = new List<PackagePart>
						{
							new PackagePart
							{
								FieldName = field.Name,
								Length = GetRegistersCount(field.Type),
								Type = field.Type,
							}
						},
					});

					field.Checked = true;
					i++;
				}
			}

			foreach (var package in Packages) package.Construct();

			Fields.Clear();
			Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 });
			foreach (var field in Configuration.Fields) Fields.Add(field.Name, new DefField { Value = 0F, Quality = 0 });
		}

		byte GetRegistersCount(string type)
		{
			switch (type)
			{
				case nameof(Byte):
				case nameof(Int16):
				case nameof(UInt16):
					return 1;

				case nameof(Int32):
				case nameof(UInt32):
				case nameof(Single):
				case nameof(DateTime):
				default:
					return 2;

				case nameof(Int64):
				case nameof(UInt64):
				case nameof(Double):
				case "Int.Int":
				case "TM2Date":
					return 4;
			}
		}

		bool Connect()
		{
			if (!Active) return false;

			try
			{
				if (Client?.Connected != true)
				{
					foreach (var f in Fields)
					{
						if (f.Key != "Time") f.Value.Quality = 0;
					}
					UpdateEvent();

					try
					{
						Client?.Close();
						Client = null;

						Stream = null;
					}
					catch { }

					Client = new TcpClient();
					Client.Connect(Configuration.Ip, Configuration.Port);

					if (Client?.Connected == true)
					{
						if (Stream == null)
						{
							Stream = Client.GetStream();
						}
					}

					//Listener = new TcpListener(IPAddress.Loopback, Configuration.Port);
					//Listener.Start();
				}

				if (Client?.Connected == true && Stream != null) return true;

				return false;
			}
			catch (Exception e)
			{
				LogEvent("Ошибка подключения: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				return false;
			}
		}

		void Monitoring()
		{
			RequestStart = DateTime.Now;
			bool isConnected = Connect();
			bool hasErr = false;

			if (isConnected)
			{
				foreach (Package package in Packages)
				{
					if (!Active) break;
					try
					{
						// Запись команды
						byte[] command = package.Trancieve;
						Stream.Write(command, 0, command.Length);
						LogEvent("Tx: " + Helpers.BytesToString(command), LogType.DETAILED);

						// Задание на считывание ответа с ожиданием
						byte[] answer = new byte[package.ReceiveLength];
						var task = Stream.ReadAsync(answer, 0, package.ReceiveLength);

						// Ожидание ответа (либо 2 секунды до продолжения)
						DateTime d = DateTime.Now;
						while (Active && (DateTime.Now - d).TotalSeconds < 2 && !task.IsCompleted)
						{
							Task.Delay(100).Wait();
						}

						// Разбор ответа
						if (task.IsCompleted)
						{
							LogEvent("Rx: " + Helpers.BytesToString(answer), LogType.DETAILED);
							if (command[0] == answer[0] && command[1] == answer[1])
							{
								package.Receive(answer);
								foreach (PackagePart part in package.Parts)
								{
									if (Fields.ContainsKey(part.FieldName))
									{
										Fields[part.FieldName].Value = part.Value;
										Fields[part.FieldName].Quality = 192;
									}
								}
							}
							else
							{
								LogEvent("Ответ не подходит");
							}
						}
						else
						{
							try { task.Dispose(); } catch { }
							LogEvent("Данные не вернулись по таймауту", LogType.DETAILED);
							hasErr = true;
						}
					}
					catch (Exception e)
					{
						LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
						hasErr = true;
					}
				}
			}

			if (hasErr) ErrCount++; else ErrCount = 0;
			if (ErrCount > 5)
			{
				try { Client.Close(); } catch { }
				try { Client = null; } catch { }
				try { Stream = null; } catch { }
				return;
			}

			if (!hasErr)
			{
				Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
				UpdateEvent();
			}
			double ms = (DateTime.Now - RequestStart).TotalMilliseconds;

			int timeout = Convert.ToInt32(Configuration.CyclicTimeout - ms);
			if (timeout > 0)
			{
				Task.Delay(timeout).Wait();
			}
		}

		bool Err(string text)
		{
			LogEvent(text, LogType.ERROR);
			return false;
		}
	}
}