using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datalake.PrivateApi.Settings;

public static class JsonSettings
{
	/// <summary>
	/// Настройки сериализации JSON, универсальные для всего приложения
	/// </summary>
	public static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};
}
