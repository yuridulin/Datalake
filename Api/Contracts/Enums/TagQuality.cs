namespace Datalake.Contracts.Public.Enums;

/// <summary>
/// Достоверность значения
/// </summary>
public enum TagQuality : byte
{
	/// <summary>
	/// Неизвестная достоверность
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Недостоверно из-за потери связи
	/// </summary>
	Bad_NoConnect = 4,

	/// <summary>
	/// Недостоверно, потому что данные не были получены
	/// </summary>
	Bad_NoValues = 8,

	/// <summary>
	/// Недостоверно - ошибка вычисления значения по формуле в случае вычисляемых тегов
	/// </summary>
	Bad_CalcError = 12,

	/// <summary>
	/// Недостоверно после ручного ввода
	/// </summary>
	Bad_ManualWrite = 26,

	/// <summary>
	/// Протянуто с последнего наблюдения
	/// </summary>
	Bad_LOCF = 100,

	/// <summary>
	/// Достоверно
	/// </summary>
	Good = 192,

	/// <summary>
	/// Протянуто с последнего наблюдения
	/// </summary>
	Good_LOCF = 200,

	/// <summary>
	/// Достоверно после ручного ввода
	/// </summary>
	Good_ManualWrite = 216,
}
