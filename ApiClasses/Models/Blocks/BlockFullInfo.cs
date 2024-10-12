using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.AccessRights;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Blocks;

/// <summary>
/// Информация о блоке
/// </summary>
public class BlockFullInfo : BlockWithTagsInfo
{
	/// <summary>
	/// Информация о родительском блоке
	/// </summary>
	public BlockParentInfo? Parent { get; set; }

	/// <summary>
	/// Список дочерних блоков
	/// </summary>
	[Required]
	public IEnumerable<BlockChildInfo> Children { get; set; } = [];

	/// <summary>
	/// Список статических свойств блока
	/// </summary>
	[Required]
	public IEnumerable<BlockPropertyInfo> Properties { get; set; } = [];

	/// <summary>
	/// Список прав доступа, которые действуют на этот блок
	/// </summary>
	[Required]
	public IEnumerable<AccessRightsForObjectInfo> AccessRights { get; set; } = [];


	/// <summary>
	/// Информация о родительском блоке
	/// </summary>
	public class BlockParentInfo : BlockNestedItem { }

	/// <summary>
	/// Информация о дочернем блоке
	/// </summary>
	public class BlockChildInfo : BlockNestedItem { }

	/// <summary>
	/// Информация о статическом свойстве блока
	/// </summary>
	public class BlockPropertyInfo : BlockNestedItem
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
