namespace Datalake.Database.Interfaces;
/// <summary>
/// Модель связи тега и тега-источника, защищенная от записи
/// </summary>
public interface IReadOnlyTagInput
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	int Id { get; set; }

	/// <summary>
	/// Идентификатор результиющего тега
	/// </summary>
	int TagId { get; set; }

	/// <summary>
	/// Идентификатор входного тега
	/// </summary>
	int? InputTagId { get; set; }

	/// <summary>
	/// Имя переменной в формуле
	/// </summary>
	string VariableName { get; set; }
}