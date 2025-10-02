namespace Datalake.Inventory.Infrastructure.Database.Schema;

/// <summary>
/// Константы базы данных с внешними данными (пользователи EnergoId)
/// </summary>
public static class EnergoIdDefinitions
{
	/// <summary>
	/// Название схемы 
	/// </summary>
	public static string Schema { get; } = "energo-id";

	/// <summary>
	/// Представление пользователей
	/// </summary>
	public static class UsersView
	{
		public static string ViewName { get; } = "users";
	}
}
