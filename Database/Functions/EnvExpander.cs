using System.Collections;
using System.Reflection;

namespace Datalake.Database.Functions;

public static class EnvExpander
{
	public static void ExpandEnvVariables(object obj)
	{
		if (obj == null)
			return;

		var type = obj.GetType();

		if (obj is string str)
		{
			throw new InvalidOperationException(
					"Direct string processing is not supported here — use property expansion."
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
				throw new Exception("Expected ENV variable is not found: " + variable);
			else
				sourceString = sourceString.Replace($"${{{variable}}}", value);
		}

		return sourceString;
	}
}
