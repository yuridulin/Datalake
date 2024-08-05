using Datalake.Database.Repositories;

namespace Server.BackgroundServices.SettingsHandler
{
	/// <summary>
	/// Интерфейс, при помощи которого можно выполнять обновление настроек из БД
	/// </summary>
	public interface ISettingsUpdater
	{
		/// <summary>
		/// Определение настроек, передаваемых веб-консоли
		/// </summary>
		/// <param name="systemRepository">Репозиторий для работы с настройками</param>
		Task WriteStartipFileAsync(SystemRepository systemRepository);

		/// <summary>
		/// Обновление списка статичных учетных записей
		/// </summary>
		/// <param name="usersRepository">Репозиторий для работы с учетными данными</param>
		void LoadStaticUsers(UsersRepository usersRepository);
	}
}
