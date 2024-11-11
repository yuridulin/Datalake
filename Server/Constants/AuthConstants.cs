namespace Datalake.Server.Constants;

/// <summary>
/// Константы, используемые для передачи данных о пользователе при выполнении запросов
/// </summary>
public static class AuthConstants
{
	/// <summary>
	/// Имя служебной переменной, через которую передаются данных о сессии между обработчиками
	/// </summary>
	public const string ContextSessionKey = "User";

	/// <summary>
	/// Название заголовка, через который передается токен сессии
	/// </summary>
	public const string TokenHeader = "D-Access-Token";

	/// <summary>
	/// Название заголовка, через который передается имя учетной записи
	/// </summary>
	public const string NameHeader = "D-Name";

	/// <summary>
	/// Название заголовка, в котором записывается глобальный уровень доступа пользователя
	/// </summary>
	public const string GlobalAccessHeader = "D-Access-Type";

	/// <summary>
	/// Название заголовка, через который передается идентификатор пользователя внешнего приложения
	/// </summary>
	public const string UnderlyingUserGuidHeader = "D-As-User";
}
