namespace Datalake.PublicApi.Constants;

/// <summary>
/// Константы, используемые для передачи данных о пользователе при выполнении запросов
/// </summary>
public static class AuthConstants
{
	/// <summary>
	/// Название заголовка, через который передается токен сессии
	/// </summary>
	public static string TokenHeader { get; } = "D-Access-Token";

	/// <summary>
	/// Название заголовка, через который передается имя учетной записи
	/// </summary>
	public static string NameHeader { get; } = "D-Name";

	/// <summary>
	/// Название заголовка, в котором записывается глобальный уровень доступа пользователя
	/// </summary>
	public static string GlobalAccessHeader { get; } = "D-Access-Type";

	/// <summary>
	/// Название заголовка, через который передается идентификатор пользователя внешнего приложения
	/// </summary>
	public static string UnderlyingUserGuidHeader { get; } = "D-As-User";
}
