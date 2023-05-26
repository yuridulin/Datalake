using System;

namespace Logger.Library
{
	public class AgentActionNtp
	{
		public int Interval { get; set; } = 5;

		public string Template { get; set; } = string.Empty;

		public string Computer { get; set; } = string.Empty;

		public int Samples { get; set; } = 5;

		public float Value { get; set; } = 1;

		// Реализация периодичности срабатывания

		DateTime LastExecute { get; set; } = DateTime.MinValue;

		public bool IsTimedOut(DateTime date)
		{
			return Interval < (date - LastExecute).TotalSeconds;
		}

		public void Restart(DateTime date)
		{
			LastExecute = date;
		}
	}
}