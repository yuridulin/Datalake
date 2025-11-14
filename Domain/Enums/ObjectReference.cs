namespace Datalake.Domain.Enums;

/// <summary>
/// Указание, объект какого типа предполагается
/// </summary>
public enum ObjectReference
{
	/// <summary>
	/// Без объекта
	/// </summary>
	None = 0,

	/// <summary>
	/// Источник данных
	/// </summary>
	Source = 1,

	/// <summary>
	/// Блок
	/// </summary>
	Block = 2,

	/// <summary>
	/// Тег
	/// </summary>
	Tag = 3,
}
