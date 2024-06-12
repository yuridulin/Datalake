namespace Datalake.ApiClasses.Models.Settings;

/// <summary>
/// Информация о настройках приложения, задаваемых через UI
/// </summary>
public class SettingsInfo
{
	/// <summary>
	/// Адрес сервера EnergoId, к которому выполняются подключения, включая порт при необходимости
	/// <br/>
	/// Протокол будет выбран на основе того, какой используется в клиенте в данный момент
	/// </summary>
	public required string EnergoIdHost { get; set; }
}
