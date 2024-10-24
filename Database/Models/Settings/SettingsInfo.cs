using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Settings;

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
	[Required]
	public required string EnergoIdHost { get; set; }

	/// <summary>
	/// Название клиента EnergoId, через который идет аутентификация
	/// </summary>
	[Required]
	public required string EnergoIdClient { get; set; }

	/// <summary>
	/// Конечная точка сервиса, который отдает информацию о пользователях EnergoId
	/// </summary>
	[Required]
	public required string EnergoIdApi { get; set; }

	/// <summary>
	/// Пользовательское название базы данных
	/// </summary>
	[Required]
	public required string InstanceName { get; set; }
}
