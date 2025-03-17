using Datalake.PublicApi.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице источников
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class Source
{
	const string TableName = "Sources";

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	[Column, NotNull]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Описание
	/// </summary>
	[Column]
	public string? Description { get; set; }

	/// <summary>
	/// Тип получения данных
	/// </summary>
	[Column]
	public SourceType Type { get; set; } = SourceType.Inopc;

	/// <summary>
	/// Адрес источника данных
	/// </summary>
	[Column]
	public string? Address { get; set; }

	// связи

	/// <summary>
	/// Список тегов, получающих значения
	/// </summary>
	public ICollection<Tag> Tags { get; set; } = [];

	/// <summary>
	/// Список правил доступа, действующих на источник
	/// </summary>
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];
}
