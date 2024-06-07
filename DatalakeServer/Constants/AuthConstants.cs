namespace DatalakeServer.Constants
{
	/// <summary>
	/// Константы, используемые для передачи данных о пользователе при выполнении запросов
	/// </summary>
	public static class AuthConstants
	{
		/// <summary>
		/// Название заголовка, через который передается имя для входа
		/// </summary>
		public const string NameHeader = "D-Name";

		/// <summary>
		/// Название заголовока, через который передается токен сессии
		/// </summary>
		public const string TokenHeader = "D-Access-Token";

		/// <summary>
		/// Название заголовка, через который передается глобальный уровень доступа пользователя
		/// </summary>
		public const string AccessHeader = "D-Access-Type";

		/// <summary>
		/// Имя служебной переменной, через которую передаются данных о сессии между обработчиками
		/// </summary>
		public const string ContextSessionKey = "User";
	}
}
