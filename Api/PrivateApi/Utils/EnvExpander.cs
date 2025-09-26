using System.Collections;
using System.Reflection;

namespace Datalake.PrivateApi.Utils;

/// <summary>
/// Распространение переменных ENV по строковым шаблонам
/// </summary>
public static class EnvExpander
{
	/// <summary>
	/// Обработка текстовых полей объекта
	/// </summary>
	public static void ExpandEnvVariables(object obj)
	{
		if (obj == null)
			return;

		var type = obj.GetType();

		if (obj is string)
		{
			throw new InvalidOperationException(
				"Этот метод предназначен для объектов. Для строк нужен метод " + nameof(FillEnvVariables) + "."
			);
		}

		if (obj is IEnumerable seq && obj is not string)
		{
			foreach (var item in seq)
				if (item != null && !IsSimpleType(item.GetType()))
					ExpandEnvVariables(item);
			return;
		}

		foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!prop.CanRead || !prop.CanWrite)
				continue;

			var value = prop.GetValue(obj);

			if (value is string s)
			{
				var replaced = FillEnvVariables(s);
				if (!ReferenceEquals(s, replaced))
					prop.SetValue(obj, replaced);
			}
			else if (value != null && !IsSimpleType(prop.PropertyType))
			{
				ExpandEnvVariables(value);
			}
		}
	}

	private static bool IsSimpleType(Type type) =>
		type.IsPrimitive ||
		type.IsEnum ||
		type.Equals(typeof(string)) ||
		type.Equals(typeof(decimal)) ||
		type.Equals(typeof(DateTime)) ||
		type.Equals(typeof(DateTimeOffset)) ||
		type.Equals(typeof(TimeSpan)) ||
		type.Equals(typeof(Guid));

	/// <summary>
	/// Обработка строки
	/// </summary>
	public static string FillEnvVariables(string sourceString)
	{
		var env = Environment.GetEnvironmentVariables();
		foreach (var part in sourceString.Split("${"))
		{
			int endSymbol = part.IndexOf('}');
			if (!part.Contains('}'))
				continue;

			string variable = part[..endSymbol];
			var value = env.Contains(variable!) ? env[variable!]?.ToString() : null;
			if (string.IsNullOrEmpty(value))
				throw new Exception("Не указана переменная окружения: " + variable);
			else
				sourceString = sourceString.Replace($"${{{variable}}}", value);
		}

		return sourceString;
	}
}
