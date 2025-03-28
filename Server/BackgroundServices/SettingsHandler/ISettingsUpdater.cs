using Datalake.Database;

namespace Datalake.Server.BackgroundServices.SettingsHandler
{
	/// <summary>
	/// Интерфейс, при помощи которого можно выполнять обновление настроек из БД
	/// </summary>
	public interface ISettingsUpdater
	{
		/// <summary>
		/// Определение настроек, передаваемых веб-консоли
		/// </summary>
		/// <param name="db">Контекст базы данных</param>
		Task WriteStartipFileAsync(DatalakeContext db);

		/// <summary>
		/// Обновление списка статичных учетных записей
		/// </summary>
		/// <param name="db">Контекст базы данных</param>
		void LoadStaticUsers(DatalakeContext db);
	}
}
