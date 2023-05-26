using System;

namespace Logger.Library
{
	public class AgentActionSqlComparer
	{
		public string Parameter { get; set; }

		public string Comparer { get; set; }

		public string Value { get; set; }

		public string Template { get; set; }

		public int RepeatInterval { get; set; }

		// Реализация выполнения

		string OldValue { get; set; }

		DateTime PreviousCheck { get; set; } = DateTime.MinValue;

		public bool IsComparerPassed(string value, DateTime date)
		{
			bool passed = false;

			if (Comparer == "==")
			{
				passed = value == (string.IsNullOrEmpty(Value) ? OldValue : Value);
			}
			else if (Comparer == "!=")
			{
				passed = value != (string.IsNullOrEmpty(Value) ? OldValue : Value);
			}
			else
			{
				if (double.TryParse(value, out double newValue) && double.TryParse((string.IsNullOrEmpty(Value) ? OldValue : Value), out double oldValue))
				{
					if (Comparer == ">")
					{
						passed = newValue > oldValue;
					}
					else if (Comparer == "<")
					{
						passed = newValue < oldValue;
					}
					else if (Comparer == ">=")
					{
						passed = newValue >= oldValue;
					}
					else if (Comparer == ">=")
					{
						passed = newValue <= oldValue;
					}
				}
			}

			OldValue = value;

			if (passed)
			{
				// условие выполняется
				if (RepeatInterval > 0)
				{
					// есть интервал срабатывания
					if (PreviousCheck == DateTime.MinValue)
					{
						PreviousCheck = date;
					}
					if ((date - PreviousCheck).TotalSeconds >= RepeatInterval)
					{
						// интервал пройден
						PreviousCheck = date;
						return true;
					}
					else
					{
						// интервал не пройден
						return false;
					}
				}
				else
				{
					// нет интервала срабатывая = каждый раз
					return true;
				}
			}
			else
			{
				// условие не выполняется
				return false;
			}
		}
	}
}