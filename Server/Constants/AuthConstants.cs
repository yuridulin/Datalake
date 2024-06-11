namespace DatalakeServer.Constants
{
	/// <summary>
	/// Константы, используемые для передачи данных о пользователе при выполнении запросов
	/// </summary>
	public static class AuthConstants
	{
		/// <summary>
		/// Название заголовока, через который передается токен сессии
		/// </summary>
		public const string TokenHeader = "D-Access-Token";

		/// <summary>
		/// Имя служебной переменной, через которую передаются данных о сессии между обработчиками
		/// </summary>
		public const string ContextSessionKey = "User";
	}
}
