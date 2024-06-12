using System.ComponentModel;

namespace Datalake.Database.Extensions;

public static class EnumExtension
{
	static public string GetDescription(this Enum enumValue)
	{
		var field = enumValue.GetType().GetField(enumValue.ToString());
		if (field == null)
			return enumValue.ToString();

		if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
			return attribute.Description;

		return enumValue.ToString();
	}
}
