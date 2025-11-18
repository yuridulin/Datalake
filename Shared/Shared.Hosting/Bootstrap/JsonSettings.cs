using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datalake.Shared.Hosting.Bootstrap;

/// <summary>
/// Настройки сериализации JSON, универсальные для всего приложения
/// </summary>
public static class JsonSettings
{
	/// <summary>
	/// Настройки сериализации JSON, универсальные для всего приложения
	/// </summary>
	public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
	{
		NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};
}
