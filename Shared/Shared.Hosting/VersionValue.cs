using System.Reflection;

namespace Datalake.Shared.Hosting;

/// <summary>
/// Значение версии приложения
/// </summary>
public class VersionValue
{
	private string _version;

	/// <summary>
	/// Конструктор
	/// </summary>
	public VersionValue()
	{
		_version = Environment.GetEnvironmentVariable("APP_VERSION")
			?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
			?? "";
	}

	/// <summary>
	/// Усеченная версия (major.minor)
	/// </summary>
	public string Short()
	{
		if (string.IsNullOrWhiteSpace(_version))
			return _version ?? string.Empty;

		var parts = _version.Split('.');
		return parts.Length >= 2
			? $"{parts[0]}.{parts[1]}"
			: _version;
	}

	/// <summary>
	/// Полная версия
	/// </summary>
	public string Full()
	{
		return _version ?? string.Empty;
	}
}
