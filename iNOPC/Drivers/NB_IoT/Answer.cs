namespace iNOPC.Drivers.NB_IoT
{
	public class Answer
	{
		public AnswerMessage Message { get; set; }

		public AnswerTelemetry Telemetry { get; set; }
	}

	public class AnswerMessage
	{
		/// <summary>
		/// тип устройства, [текст]
		/// </summary>
		public string dev { get; set; }

		/// <summary>
		/// Imei модема, [текст]
		/// </summary>
		public string imei { get; set; }

		/// <summary>
		/// номер пакета, [целое число]
		/// </summary>
		public int num { get; set; }
	}

	public class AnswerTelemetry
	{
		/// <summary>
		/// дата в формате UTC, [целое число]
		/// </summary>
		public long date { get; set; }

		/// <summary>
		/// уровень сигнала, [целое число]
		/// </summary>
		public int rssi { get; set; }

		/// <summary>
		/// заряд батареи в мВ, [целое число]
		/// </summary>
		public int bat_mv { get; set; }

		/// <summary>
		/// температура на модеме, [целое число]
		/// </summary>
		public int temp { get; set; }

		/// <summary>
		/// сопротивление проводника на канале А, Ом [целое число]
		/// </summary>
		public int raw { get; set; }

		/// <summary>
		/// сопротивление изоляции на канале А, кОм [целое число]
		/// </summary>
		public int rai { get; set; }

		/// <summary>
		/// сопротивление проводника на канале B, Ом [целое число]
		/// </summary>
		public int rbw { get; set; }

		/// <summary>
		/// сопротивление изоляции на канале B, кОм [целое число]
		/// </summary>
		public int rbi { get; set; }

		/// <summary>
		/// сопротивление проводника на канале C, Ом [целое число]
		/// </summary>
		public int rcw { get; set; }

		/// <summary>
		/// сопротивление изоляции на канале C, кОм [целое число]
		/// </summary>
		public int rci { get; set; }

		/// <summary>
		/// сопротивление проводника на канале D, Ом [целое число]
		/// </summary>
		public int rdw { get; set; }

		/// <summary>
		/// сопротивление изоляции на канале D, кОм [целое число]
		/// </summary>
		public int rdi { get; set; }

		/// <summary>
		/// состояние входа 1 [целое число]
		/// </summary>
		public int di1 { get; set; }

		/// <summary>
		/// состояние входа 2 [целое число]
		/// </summary>
		public int di2 { get; set; }
	}
}
