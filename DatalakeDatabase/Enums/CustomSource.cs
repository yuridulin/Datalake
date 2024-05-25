using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DatalakeDatabase.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CustomSource
{
	[Description("Системные теги с данными о текущей работе различных частей приложения")]
	System = 0,

	[Description("Пользовательские теги, значения которых вычисляются по формулам на основе значений других тегов")]
	Calculated = -1,

	[Description("Пользовательские теги с ручным вводом значений в произвольный момент времени")]
	Manual = -2,

	[Description("Заглушка для неопределённого источника")]
	NotSet = -666,
}
