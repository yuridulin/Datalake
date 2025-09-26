using System.Reflection;

namespace Datalake.PrivateApi.ValueObjects;

public class VersionValue
{
	private string _version;

	public VersionValue()
	{
		_version = Environment.GetEnvironmentVariable("APP_VERSION")
			?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
			?? "";
	}

	/// <summary>
	/// Обрезка версии до первых двух цифр (major.minor)
	/// </summary>
	/// <param name="version">Текущая версия</param>
	/// <returns>Усеченная версия</returns>
	public string Short()
	{
		if (string.IsNullOrWhiteSpace(_version))
			return _version ?? string.Empty;

		var parts = _version.Split('.');
		return parts.Length >= 2
			? $"{parts[0]}.{parts[1]}"
			: _version;
	}

	public string Full()
	{
		return _version ?? string.Empty;
	}
}
