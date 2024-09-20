using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Blocks;

/// <summary>
/// Информация о блоке
/// </summary>
public class BlockInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	[Required]
	public int Id { get; set; } = 0;

	/// <summary>
	/// Наименование
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Текстовое описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Информация о родительском блоке
	/// </summary>
	public BlockParentInfo? Parent { get; set; }

	/// <summary>
	/// Список дочерних блоков
	/// </summary>
	[Required]
	public BlockChildInfo[] Children { get; set; } = [];

	/// <summary>
	/// Список статических свойств блока
	/// </summary>
	[Required]
	public BlockPropertyInfo[] Properties { get; set; } = [];

	/// <summary>
	/// Список прикреплённых тегов
	/// </summary>
	[Required]
	public BlockTagInfo[] Tags { get; set; } = [];


	/// <summary>
	/// Связанный с блоком объект
	/// </summary>
	public class BlockRelationInfo
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[Required]
		public int Id { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
		[Required]
		public required string Name { get; set; }
	}

	/// <summary>
	/// Информация о родительском блоке
	/// </summary>
	public class BlockParentInfo : BlockRelationInfo { }

	/// <summary>
	/// Информация о дочернем блоке
	/// </summary>
	public class BlockChildInfo : BlockRelationInfo { }

	/// <summary>
	/// Информация о закреплённом теге
	/// </summary>
	public class BlockTagInfo : BlockRelationInfo
	{
		/// <summary>
		/// Идентификатор тега
		/// </summary>
		[Required]
		public required Guid Guid { get; set; }

		/// <summary>
		/// Тип поля блока для этого тега
		/// </summary>
		public BlockTagRelation Relation { get; set; } = BlockTagRelation.Static;

		/// <summary>
		/// Тип значений тега
		/// </summary>
		public TagType TagType { get; set; } = TagType.String;

		/// <summary>
		/// Свое имя тега в общем списке
		/// </summary>
		public string TagName { get; set; } = string.Empty;
	}

	/// <summary>
	/// Информация о статическом свойстве блока
	/// </summary>
	public class BlockPropertyInfo : BlockRelationInfo
	{
		/// <summary>
		/// Тип значения свойства
		/// </summary>
		[Required]
		public required TagType Type { get; set; }

		/// <summary>
		/// Значение свойства
		/// </summary>
		[Required]
		public required string Value { get; set; }
	}
}
