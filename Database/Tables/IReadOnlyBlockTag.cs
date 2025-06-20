using Datalake.PublicApi.Enums;

namespace Datalake.Database.Tables;

/// <summary>
/// Модель связи блока с тегов, защищенная от записи
/// </summary>
public interface IReadOnlyBlockTag
{
	/// <summary>
	/// Идентификатор блока
	/// </summary>
	int BlockId { get; set; }

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	int? TagId { get; set; }

	/// <summary>
	/// Название в рамках блока
	/// </summary>
	string? Name { get; set; }

	/// <summary>
	/// Тип связи тега к блоку
	/// </summary>
	BlockTagRelation Relation { get; set; }
}