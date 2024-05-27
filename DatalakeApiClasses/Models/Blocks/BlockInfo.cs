using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Blocks;

/// <summary>
/// Информация о сущности
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
	/// Информация о родительской сущности
	/// </summary>
	public BlockParentInfo? Parent { get; set; }

	/// <summary>
	/// Список дочерних сущностей
	/// </summary>
	[Required]
	public BlockChildInfo[] Children { get; set; } = [];

	/// <summary>
	/// Список статических свойств сущности
	/// </summary>
	[Required]
	public BlockPropertyInfo[] Properties { get; set; } = [];

	/// <summary>
	/// Список прикреплённых тегов
	/// </summary>
	[Required]
	public BlockTagInfo[] Tags { get; set; } = [];


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
	/// Информация о родительской сущности
	/// </summary>
	public class BlockParentInfo : BlockRelationInfo { }

	/// <summary>
	/// Информация о дочерней сущности
	/// </summary>
	public class BlockChildInfo : BlockRelationInfo { }

	/// <summary>
	/// Информация о закреплённом теге
	/// </summary>
	public class BlockTagInfo : BlockRelationInfo
	{
		/// <summary>
		/// Тип значений тега
		/// </summary>
		[Required]
		public required BlockTagRelation TagType { get; set; }
	}

	/// <summary>
	/// Информация о статическом свойстве сущности
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
