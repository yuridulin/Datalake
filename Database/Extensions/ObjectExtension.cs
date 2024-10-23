namespace Datalake.Database.Extensions
{
	public static class ObjectExtension
	{
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
}
