namespace Datalake.ApiClasses.Enums;

/// <summary>
/// Категория, к которой относится сообщение
/// </summary>
public enum LogCategory
{
	/// <summary>
	/// Базовое приложение
	/// </summary>
	Core = 0,

	/// <summary>
	/// Работа с базой данных
	/// </summary>
	Database = 10,

	/// <summary>
	/// Получение данных с источников
	/// </summary>
	Collector = 20,

	/// <summary>
	/// API-запросы
	/// </summary>
	Api = 30,

	/// <summary>
	/// Вычисления
	/// </summary>
	Calc = 40,

	/// <summary>
	/// Источники данных
	/// </summary>
	Source = 50,

	/// <summary>
	/// Теги
	/// </summary>
	Tag = 60,

	/// <summary>
	/// Транспортный уровень
	/// </summary>
	Http = 70,

	/// <summary>
	/// Пользователи
	/// </summary>
	Users = 80,

	/// <summary>
	/// Группы пользователей
	/// </summary>
	UserGroups = 90,
}
