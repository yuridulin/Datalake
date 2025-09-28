namespace Datalake.InventoryService.Domain.Constants;

/// <summary>
/// Константы базы данных с внешними данными (пользователи EnergoId)
/// </summary>
public static class EnergoIdDefinitions
{
	/// <summary>
	/// Название схемы 
	/// </summary>
	public const string Schema = "energo-id";

	/// <summary>
	/// Представление пользователей
	/// </summary>
	public static class UsersView
	{
		public const string ViewName = "users";
	}
}
