namespace Datalake.Domain.Enums;

/// <summary>
/// Тип связи тега и блока
/// </summary>
public enum BlockTagRelation
{
	/// <summary>
	/// Статичное закрепление (простое свойство)
	/// </summary>
	Static = 0,

	/// <summary>
	/// Может использоваться как переменная в формулах в пределах локальной области блока
	/// </summary>
	Input = 1,

	/// <summary>
	/// Может использоваться как переменная в других блоках
	/// </summary>
	Output = 2,
}
