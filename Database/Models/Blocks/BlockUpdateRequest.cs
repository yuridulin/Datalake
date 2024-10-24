using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Blocks;

/// <summary>
/// Новая информация о блоке
/// </summary>
public class BlockUpdateRequest
{
	/// <summary>
	/// Новое название
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Новое описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Новый список закрепленных тегов
	/// </summary>
	[Required]
	public required AttachedTag[] Tags { get; set; }

	/// <summary>
	/// Информация о закрепленном теге
	/// </summary>
	public class AttachedTag
	{
		/// <summary>
		/// Локальный идентификатор тега
		/// </summary>
		[Required]
		public required int Id { get; set; }

		/// <summary>
		/// Название поля в блоке, которому соответствует тег
		/// </summary>
		[Required]
		public required string Name { get; set; }

		/// <summary>
		/// Тип поля блока
		/// </summary>
		[Required]
		public BlockTagRelation Relation { get; set; } = BlockTagRelation.Static;
	}
}
