namespace Datalake.Inventory.Infrastructure.Database.Schema;

/// <summary>
/// Константы базы данных с внешними данными (пользователи EnergoId)
/// </summary>
public static class EnergoIdSchema
{
	/// <summary>
	/// Название схемы 
	/// </summary>
	public static string Name { get; } = "energo-id";

	/// <summary>
	/// Представление пользователей
	/// </summary>
	public static class EnergoId
	{
		public static string Name { get; } = "Users";
	}
}
