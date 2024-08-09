namespace Datalake.Database.Extensions
{
	public class ObjectExtension<T> where T : class
	{
		public string Difference(T old, T @new)
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
