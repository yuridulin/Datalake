using System.Collections;

namespace Datalake.PrivateApi.Extensions;

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
	/// <param name="collectionKeys">Ключи для сравнения коллекций</param>
	/// <returns>Строковая запись различий</returns>
	public static string Difference<T>(T old, T @new, Dictionary<string, Func<object, object>>? collectionKeys = null) where T : class
	{
		var type = typeof(T);
		List<string> parts = [];

		foreach (var property in type.GetProperties())
		{
			var oldValue = property.GetValue(old);
			var newValue = property.GetValue(@new);

			if (IsCollection(property.PropertyType))
			{
				var oldCollection = oldValue as IEnumerable;
				var newCollection = newValue as IEnumerable;

				if (collectionKeys != null)
				{
					var keySelector = collectionKeys.GetValueOrDefault(property.Name);
					if (keySelector != null)
					{
						string collectionDiff = CompareCollections(
								property.Name,
								oldCollection,
								newCollection,
								keySelector,
								collectionKeys
						);

						if (!string.IsNullOrEmpty(collectionDiff))
							parts.Add(collectionDiff);
					}
				}
			}
			else
			{
				if (!Equals(oldValue, newValue))
					parts.Add($"{property.Name}: [{oldValue}] -> [{newValue}]");
			}
		}

		return parts.Count > 0 ? string.Join(", ", parts) : "Изменений нет";
	}

	private static bool IsCollection(Type type)
	{
		return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
	}

	private static string CompareCollections(
			string propertyName,
			IEnumerable? oldCollection,
			IEnumerable? newCollection,
			Func<object, object> keySelector,
			Dictionary<string, Func<object, object>> collectionKeys)
	{
		if (oldCollection == null && newCollection == null)
			return string.Empty;
		if (oldCollection == null)
			return $"{propertyName}: Коллекция добавлена";
		if (newCollection == null)
			return $"{propertyName}: Коллекция удалена";

		var oldDict = ToDictionary(oldCollection, keySelector);
		var newDict = ToDictionary(newCollection, keySelector);

		var added = newDict.Keys.Except(oldDict.Keys).ToList();
		var removed = oldDict.Keys.Except(newDict.Keys).ToList();
		var common = oldDict.Keys.Intersect(newDict.Keys).ToList();

		var changes = new List<string>();
		foreach (var key in common)
		{
			var oldItem = oldDict[key];
			var newItem = newDict[key];

			string diff = Difference(oldItem, newItem, collectionKeys);
			if (diff != "Изменений нет")
				changes.Add($"{key}: {diff}");
		}

		List<string> result = [];
		if (added.Count > 0)
			result.Add($"Добавлено: {string.Join(", ", added)}");
		if (removed.Count > 0)
			result.Add($"Удалено: {string.Join(", ", removed)}");
		if (changes.Count > 0)
			result.Add($"Изменено: {string.Join("; ", changes)}");

		return result.Count > 0 ? $"{propertyName}: {string.Join("; ", result)}" : string.Empty;
	}

	private static Dictionary<object, object> ToDictionary(IEnumerable collection, Func<object, object> keySelector)
	{
		var dict = new Dictionary<object, object>();
		if (collection == null)
			return dict;

		foreach (var item in collection)
		{
			var key = keySelector?.Invoke(item) ?? item;
			if (key != null && !dict.ContainsKey(key))
				dict[key] = item;
		}
		return dict;
	}
}
