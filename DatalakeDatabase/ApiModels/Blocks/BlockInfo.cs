namespace DatalakeDatabase.ApiModels.Blocks;

/// <summary>
/// Информация о сущности
/// </summary>
public class BlockInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; } = 0;

	/// <summary>
	/// Наименование
	/// </summary>
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
	public BlockChildInfo[] Children { get; set; } = [];

	/// <summary>
	/// Список статических свойств сущности
	/// </summary>
	public BlockPropertyInfo[] Properties { get; set; } = [];

	/// <summary>
	/// Список прикреплённых тегов
	/// </summary>
	public BlockTagInfo[] Tags { get; set; } = [];


	public class BlockRelationInfo
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
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
	public class BlockTagInfo : BlockRelationInfo { }

	/// <summary>
	/// Информация о статическом свойстве сущности
	/// </summary>
	public class BlockPropertyInfo : BlockRelationInfo { }
}
