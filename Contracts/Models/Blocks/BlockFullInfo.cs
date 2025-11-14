using Datalake.Contracts.Models.Abstractions;
using Datalake.Contracts.Models.AccessRules;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

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
	public BlockChildInfo[] Children { get; set; } = [];

	/// <summary>
	/// Список статических свойств блока
	/// </summary>
	[Required]
	public BlockPropertyInfo[] Properties { get; set; } = [];

	/// <summary>
	/// Список прав доступа, которые действуют на этот блок
	/// </summary>
	[Required]
	public AccessRightsForObjectInfo[] AccessRights { get; set; } = [];

	/// <summary>
	/// Список родительских блоков
	/// </summary>
	[Required]
	public BlockTreeInfo[] Adults { get; set; } = [];

	/// <summary>
	/// Информация о вложенном объекте
	/// </summary>
	public abstract class BlockNestedItem : INestedItem
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public required string Name { get; set; }
	}

	/// <summary>
	/// Информация о родительском блоке
	/// </summary>
	public class BlockParentInfo : BlockNestedItem
	{
	}

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
