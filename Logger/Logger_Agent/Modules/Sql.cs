using Logger.Library;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Timers;

namespace Logger.Agent.Modules
{
	public static class Sql
	{
		public static void Start()
		{
			Timer = new Timer(10000);
			Timer.Elapsed += Timer_Elapsed;
			Timer.Start();
		}

		public static void Stop()
		{
			Timer.Stop();
			Helpers.RaiseEvent(AgentLogSources.Sql, "stopped");
		}

		// реализация

		static Timer Timer { get; set; }

		static void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				// проверяем таймауты на объектах
				var actions = Program.Config.SqlActions
					.Where(x => x.IsTimedOut(DateTime.Now) == true)
					.ToList();

				// для тех, чей таймаут вышел, запускаем выполнение
				foreach (var action in actions)
				{
					string[] parameters = new string[0];
					var values = new List<string[]>();

					if (action.DatabaseType == "SQL Server")
					{
						using (var conn = new SqlConnection())
						{
							conn.ConnectionString = action.ConnectionString;
							conn.Open();

							Helpers.RaiseEvent(AgentLogSources.Sql, "Подключение по строке \"" + action.ConnectionString + "\"");

							using (var command = new SqlCommand())
							{
								command.Connection = conn;
								command.CommandText = action.CommandCode;
								command.CommandTimeout = action.CommandTimeout;

								Helpers.RaiseEvent(AgentLogSources.Sql, "Выполнение кода c таймаутом " + action.CommandTimeout + "c\r\n\"" + action.CommandCode + "\"");

								string message = "";

								using (var reader = command.ExecuteReader())
								{
									if (reader.HasRows)
									{
										parameters = new string[reader.FieldCount];
										for (int i = 0; i < reader.FieldCount; i++)
										{
											parameters[i] = reader.GetName(i);
										}

										while (reader.Read())
										{
											string[] row = new string[reader.FieldCount];
											for (int i = 0; i < reader.FieldCount; i++)
											{
												row[i] = reader[i].ToString();
											}

											values.Add(row);
										}
									}
								}

								Helpers.RaiseEvent(AgentLogSources.Sql, "Результаты выполнения:" + message);
							}
						}
					}

					foreach (var check in action.Comparers)
					{
						Helpers.RaiseEvent(AgentLogSources.Sql, "Проверка компаратора со значением №" + check.Parameter + ": " + check.Comparer + " " + check.Value);

						int paramId = -1;
						for (int i = 0; i < parameters.Length; i++)
						{
							if (parameters[i] == check.Parameter)
							{
								paramId = i;
								break;
							}
						}

						if (paramId < 0)
						{
							Helpers.RaiseEvent(AgentLogSources.Sql, "Индекс параметра указан неправильно, проверка остановлена");
							continue;
						}

						foreach (var row in values)
						{
							string value = row[paramId];

							if (check.IsComparerPassed(value, DateTime.Now))
							{
								var message = check.Template
									.Replace("@parameter", check.Parameter.ToString())
									.Replace("@value", value)
									.Replace("@check", check.Value)
									.Replace("@comparer", check.Comparer);

								Helpers.RaiseEvent(AgentLogSources.Sql, message);
							}
						}
					}

					// взводим таймаут
					action.Restart(DateTime.Now);
				}
			}
			catch (Exception ex)
			{
				Helpers.RaiseEvent(AgentLogSources.Sql, "Ошибка\r\n" + ex.Message + "\r\n" + ex.StackTrace, true);
			}
		}
	}
}
