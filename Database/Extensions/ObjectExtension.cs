namespace Datalake.Database.Extensions;

/// <summary>
/// Расширение для работы с объектами
/// </summary>
public static class ObjectExtension
{
	/// <summary>
	/// Запись различий двух однотипных объектов
	/// </summary>
	/// <typeparam name="T">Тип объектов</typeparam>
	/// <param name="old">Исходный объект</param>
	/// <param name="new">Измененный объект</param>
	/// <returns>Строковая запись различий</returns>
	public static string Difference<T>(T old, T @new) where T : class
	{
		var type = typeof(T);
		List<string> parts = [];

		foreach (var property in type.GetProperties())
		{
			var oldValue = property.GetValue(old);
			var newValue = property.GetValue(@new);

			if (!Equals(oldValue, newValue))
			{
				parts.Add($"{property.Name}: [{oldValue}] -> [{newValue}]");
			}
		}
		if (parts.Count > 0)
		{
			return "Изменения: " + string.Join(", ", parts);
		}

		return "Изменений нет";
	}
}
