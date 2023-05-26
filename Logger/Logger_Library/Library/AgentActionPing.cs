using System;

namespace Logger.Library
{
	public class AgentActionPing
	{
		public string Target { get; set; }

		public int Interval { get; set; }

		public string Template { get; set; }

		public int Value { get; set; }

		// Реализация периодичности срабатывания

		DateTime LastPing { get; set; } = DateTime.MinValue;

		int TriesWithoutPing { get; set; } = 0;

		public bool IsTimedOut(DateTime date)
		{
			#if DEBUG
			//Console.WriteLine(
			//	$"\tping: date {date:dd.MM.yyyy HH:mm:ss} " +
			//	$"last {LastPing:dd.MM.yyyy HH:mm:ss} " +
			//	$"dif {(date - LastPing).TotalSeconds} " +
			//	$"interval {Interval} " +
			//	$"result {Interval < (date - LastPing).TotalSeconds}"
			//);
			#endif

			return Interval < (date - LastPing).TotalSeconds;
		}

		public void Restart(DateTime date)
		{
			LastPing = date;
		}

		public void AddTry()
		{
			TriesWithoutPing++;
		}

		public void ClearTries()
		{
			TriesWithoutPing = 0;
		}

		public bool IsLost()
		{
			return TriesWithoutPing >= Value;
		}
	}
}