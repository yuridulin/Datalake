using iNOPC.Drivers.MODBUS_RTU.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace iNOPC.Drivers.MODBUS_RTU
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

			try { Port?.Close(); } catch (Exception) { }
			Port = new SerialPort();
			Port.DataReceived += (s, e) =>
			{
				BytesReceived = Port.BytesToRead;
				Receive = true;
			};
			Port.ErrorReceived += (s, e) => ComError(e.EventType.ToString());

			// чтение конфигурации
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
			}
			catch (Exception e)
			{
				return Err("Конфигурация не прочитана: " + e.Message);
			}

			if (Configuration.Fields.Count == 0)
			{
				return Err("Список опрашиваемых полей пуст");
			}
			if (!Fields.ContainsKey("Time"))
			{
				Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 });
			}

			UpdateEvent();

			// подготовка пакетов для запроса
			try
			{
				Packages = new List<Package>();
				CreatePackages();
			}
			catch (Exception e)
			{
				return Err("Ошибка при обработке полей: " + e.Message + "\n" + e.StackTrace);
			}

			// установка начальных значений
			try
			{
				Port.PortName = Configuration.PortName;
				Port.BaudRate = Configuration.BaudRate;
				Port.DataBits = Configuration.DataBits;
				Port.Parity = (Parity)Configuration.Parity;
				Port.StopBits = (StopBits)Configuration.StopBits;
				Port.ReadTimeout = Configuration.ReadTimeout;
				Port.WriteTimeout = Configuration.WriteTimeout;
			}
			catch (Exception e)
			{
				return Err("Параметры COM порта не установлены: " + e.Message + "\n" + e.StackTrace);
			}

			Active = true;

			if (Thread != null)
			{
				Thread.Abort();
				Thread = null;
			}

			Thread = new Thread(TrancievePackages);
			Thread.Start();

			LogEvent("Мониторинг запущен");

			return true;
		}

		public void Stop()
		{
			LogEvent("Остановка ...");

			Active = false;
			try { Port.Close(); } catch (Exception) { }

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}


		// Реализация получения данных

		Configuration Configuration { get; set; }

		List<Package> Packages { get; set; }

		SerialPort Port { get; set; }

		Thread Thread { get; set; }

		DateTime Date { get; set; }

		DateTime PackageDate { get; set; }

		bool Active { get; set; } = false;

		bool Receive { get; set; } = false;

		int BytesReceived { get; set; } = 0;

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
				Configuration.Fields = Configuration.Fields.Where(x => x.IsActive).OrderBy(x => x.Address).ToList();

				while (Configuration.Fields.Count(x => !x.Checked) > 0)
				{
					var fields = Configuration.Fields.Where(x => !x.Checked).ToArray();

					Package package = null;
					byte length = 0;

					for (int k = 0; k < Math.Min(fields.Length, Configuration.MaxFieldsInGroup); k++)
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
				foreach (var field in Configuration.Fields.Where(x => x.IsActive).Where(x => !x.Checked).OrderBy(x => x.Address))
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
			Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss") });
			foreach (var field in Configuration.Fields) 
			{
				switch (field.Type)
				{
					case "Date": 
						Fields.Add(field.Name, new DefField { Value = "" });
						break;

					default:
						Fields.Add(field.Name, new DefField { Value = 0F });
						break;
				}
			}
		}

		void TrancievePackages()
		{
			int errorsCounter = 0;

			while (Active)
			{
				Date = DateTime.Now;

				if (!Port.IsOpen)
				{
					try
					{
						Port.Open();
					}
					catch (Exception e)
					{
						Err("COM-порт не открыт: " + e.Message + "\n" + e.StackTrace);
						try { Port?.Close(); } catch (Exception) { }
						Thread.Sleep(1000);
					}
				}

				if (Port.IsOpen)
				{
					// запросы к прибору
					foreach (Package package in Packages)
					{
						if (!Active) break;
						try
						{
							Receive = false;
							BytesReceived = 0;
							PackageDate = DateTime.Now;

							byte[] command = package.Trancieve;
							Port.Write(command, 0, command.Length);
							LogEvent("Tx: " + Helpers.BytesToString(command), LogType.DETAILED);

							while ((!Receive || BytesReceived < package.ReceiveLength) && (DateTime.Now - PackageDate).TotalMilliseconds < Configuration.ReceiveTimeout)
							{
								Thread.Sleep(1);
							}

							if (Receive)
							{
								byte[] answer = new byte[Port.BytesToRead];
								Port.Read(answer, 0, answer.Length);
								LogEvent("Rx: " + Helpers.BytesToString(answer));

								byte[] _package = answer.ToList().GetRange(0, answer.Length - 2).ToArray();
								byte[] _crc = BitConverter.GetBytes(Package.CRC(_package, _package.Length));
								byte[] _values = _package.ToList().GetRange(3, _package.Length - 3).ToArray();

								// Проверка длины посылки
								if (answer.Length != package.ReceiveLength)
								{
									LogEvent("длина ответа не совпадает: пришло " + answer.Length + ", ожидается " + package.ReceiveLength + " байт", LogType.DETAILED);
								}
								// Проверка адреса устройства
								else if (answer[0] != package.SlaveId)
								{
									LogEvent("адрес устройства не совпадает: пришло " + answer[0] + ", ожидается " + package.SlaveId, LogType.DETAILED);
								}
								// Проверка кода команды
								else if (answer[1] != package.CommandCode)
								{
									LogEvent("код команды не совпадает: пришло " + answer[1] + ", ожидается " + package.CommandCode, LogType.DETAILED);
								}
								// Проверка контрольной суммы
								else if (_crc[0] != answer[answer.Length - 2] || _crc[1] != answer[answer.Length - 1])
								{
									LogEvent("CRC не совпадает: получена " + Helpers.BytesToString(new[] { answer[answer.Length - 2], answer[answer.Length - 1], }) + ", расчетная " + Helpers.BytesToString(_crc) + " байт", LogType.DETAILED);
								}
								// Проверка количества значимых байтов
								else if (answer[2] != _values.Length)
								{
									LogEvent("кол-во байтов данных не совпадает: пришло " + answer[2] + ", ожидается " + _values.Length + " байт", LogType.DETAILED);
								}
								// Если все норм, выполняется разбор ответа
								else
								{
									string err = package.Receive(answer);
									if (err != null)
									{
										LogEvent("Разбор ответа: " + err, LogType.ERROR);
									}

									lock (Fields)
									{
										foreach (var part in package.Parts)
										{
											if (Fields.ContainsKey(part.FieldName))
											{
												Fields[part.FieldName].Value = part.Value;
												Fields[part.FieldName].Quality = 192;
											}
										}
									}
								}
							}
							else
							{
								LogEvent("Rx: ничего не вернулось, таймаут " + Configuration.ReceiveTimeout + " мс", LogType.DETAILED);
								errorsCounter++;
							}

							Thread.Sleep(5);
						}
						catch (Exception e)
						{
							LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
							errorsCounter++;
						}
					}
				}

				lock (Fields)
				{
					Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
					Fields["Time"].Quality = 192;
				}
				UpdateEvent();

				if (errorsCounter > 20 || Configuration.UseStaticConnection)
				{
					try { Port?.Close(); } catch (Exception) { }
					errorsCounter = 0;
					Thread.Sleep(1000);
				}

				int timeout = Convert.ToInt32((Configuration.CyclicTimeout * 1000) - (DateTime.Now - Date).TotalMilliseconds);
				if (timeout > 0) Thread.Sleep(timeout);
			}

			try { Port?.Close(); } catch (Exception) { }
		}

		void ComError(string errorName)
		{
			LogEvent("COM: " + errorName, LogType.DETAILED);
			Port.BaseStream.Flush();
			Port.DiscardInBuffer();
			Port.DiscardOutBuffer();
		}

		byte GetRegistersCount(string type)
		{
			switch (type)
			{
				case "Word":
				case "UInt16":
					return 1;

				case "Int":
				case "UInt32":
				case "Single":
				case "Date":
				case "AKVT_BCD":
				default:
					return 2;

				case "Long":
				case "UInt64":
				case "Double":
				case "Int.Int":
				case "TM2Date":
					return 4;
			}
		}

		bool Err(string text)
		{
			LogEvent(text, LogType.ERROR);
			return false;
		}
	}
}