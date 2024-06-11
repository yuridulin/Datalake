namespace DatalakeServer.Services.Receiver.Models.Inopc.Enums
{
	/// <summary>
	/// Достоверность значения в INOPC
	/// </summary>
	public enum InopcTagQuality
	{
		/// <summary>
		/// Недостоверно
		/// </summary>
		Bad = 0,

		/// <summary>
		/// Недостоверно из-за обрыва связи
		/// </summary>
		Bad_NoConnect = 4,

		/// <summary>
		/// Недостоверно, потому что данные не предоставлены
		/// </summary>
		Bad_NoValues = 8,

		/// <summary>
		/// Недостоверно после ручного ввода
		/// </summary>
		Bad_ManualWrite = 26,

		/// <summary>
		/// Достоверно
		/// </summary>
		Good = 192,

		/// <summary>
		/// Достоверно после ручного ввода
		/// </summary>
		Good_ManualWrite = 216,

		/// <summary>
		/// Неизвестная степень достоверности
		/// </summary>
		Unknown = -1,
	}
}
