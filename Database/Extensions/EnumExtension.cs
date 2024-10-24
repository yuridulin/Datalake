using System.ComponentModel;

namespace Datalake.Database.Extensions;

/// <summary>
/// Расширения для работы с enum
/// </summary>
public static class EnumExtension
{
	/// <summary>
	/// Получение описания значения enum
	/// </summary>
	/// <param name="enumValue">Значение</param>
	/// <returns>Описание, записанное в атрибуте Description</returns>
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
