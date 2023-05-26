using Logger.Library;
using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace Logger.Agent.Modules
{
	public static class Ntp
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
			Helpers.RaiseEvent(AgentLogSources.Ntp, "stopped");
		}

		// реализация

		static Timer Timer { get; set; }

		static void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				// проверяем таймауты на объектах
				var actions = Program.Config.NtpActions
					.Where(x => x.IsTimedOut(DateTime.Now) == true)
					.ToList();

				// для тех, чей таймаут вышел, запускаем пинг
				foreach (var action in actions)
				{
					var diff = ParseNtp(action.Computer, action.Samples);

					if (Math.Abs(diff) >= action.Value)
					{
						var message = action.Template
							.Replace("@computer", action.Computer)
							.Replace("@samples", action.Samples.ToString())
							.Replace("@diff", diff.ToString());

						Helpers.RaiseEvent(AgentLogSources.Ntp, message, true);
					}

					// взводим таймаут
					action.Restart(DateTime.Now);
				}
			}
			catch (Exception ex)
			{
				Helpers.RaiseEvent(AgentLogSources.Ntp, "Ошибка\r\n" + ex.Message + "\r\n" + ex.StackTrace, true);
			}
		}

		static float ParseNtp(string target, int samples)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "w32tm",
					Arguments = "/stripchart /computer:" + target + " /samples:" + samples + " /dataonly",
					UseShellExecute = false,
					RedirectStandardOutput = true,
				}
			};

			process.Start();
			process.WaitForExit(2000);
			var text = process.StandardOutput.ReadToEnd();

			string[] lines = text.Split(new char[] { '\n' });
			float diff = 0;
			for (int i = 3; i < lines.Length - 1; i++)
			{
				var parts = lines[i].Split(new char[] { ',' });
				//var time = DateTime.Today.Add(TimeSpan.Parse(parts[0].Trim()));
				diff = float.Parse(parts[1].Substring(0, parts[1].Length - 1).Trim().Replace(".", ","));
			}
			return diff;
		}
	}
}
