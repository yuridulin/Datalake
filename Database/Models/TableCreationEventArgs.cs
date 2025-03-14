namespace Datalake.Database.Models;

/// <summary>
/// Данные, передаваемые при создании события во внешние прослушивающие вызовы
/// </summary>
public class TableCreationEventArgs : EventArgs
{
	/// <summary>
	/// Имя созданной таблицы
	/// </summary>
	public required string TableName { get; set; }
}
