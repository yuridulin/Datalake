using Logger.Library;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Timers;

namespace Logger.Agent.Modules
{
	public static class Pings
	{
		public static void Start()
		{
			Timer = new Timer(1000);
			Timer.Elapsed += Timer_Elapsed;
			Timer.Start();
		}

		public static void Stop()
		{
			Timer.Stop();
			Helpers.RaiseEvent(AgentLogSources.Ping, "stopped");
		}

		// реализация

		static Timer Timer { get; set; }

		static void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				// проверяем таймауты на объектах
				var actions = Program.Config.Pings
					.Where(x => x.IsTimedOut(DateTime.Now) == true)
					.ToList();

				// для тех, чей таймаут вышел, запускаем пинг
				foreach (var action in actions)
				{
					Helpers.RaiseEvent(AgentLogSources.Ping, "Объект " + action.Target + ", старт проверки пинга");

					// по результату создаем сообщение из шаблона
					// если шаблон пустой - ничего не делаем
					using (var ping = new Ping())
					{
						var reply = ping.Send(action.Target, 150);

						if (reply.Status == IPStatus.Success)
						{
							Helpers.RaiseEvent(AgentLogSources.Ping, "Объект " + action.Target + ", пинг прошел");
							action.ClearTries();
						}
						else
						{
							Helpers.RaiseEvent(AgentLogSources.Ping, "Объект " + action.Target + ", пинг не прошел");
							action.AddTry();

							if (action.IsLost() == true)
							{
								Helpers.RaiseEvent(AgentLogSources.Ping, "Объект " + action.Target + ", определена потеря связи");
								if (!string.IsNullOrEmpty(action.Template))
								{
									Helpers.RaiseEvent(AgentLogSources.Ping, "Объект " + action.Target + ", формируется сообщение по шаблону: " + action.Template);

									var message = action.Template
										.Replace("@target", action.Target)
										.Replace("@status", reply.Status.ToString());

									Helpers.RaiseEvent(AgentLogSources.Ping, message);
								}
							}
						}
					}

					// взводим таймаут
					action.Restart(DateTime.Now);
				}
			}
			catch (Exception ex)
			{
				Helpers.RaiseEvent(AgentLogSources.Ping, "Ошибка\r\n" + ex.Message + "\r\n" + ex.StackTrace, true);
			}
		}
	}
}
