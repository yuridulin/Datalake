using Datalake.Database.Models;
using Datalake.Database.Utilities;
using LinqToDB;

namespace Datalake.Database.Extensions;

public static class DatalakeContextExtension
{
	/// <summary>
	/// Обновление времени последнего изменения структуры тегов, источников и блоков в базе данных
	/// </summary>
	/// <param name="db">Подключение к базе данных</param>
	public static void SetLastUpdateToNow(this DatalakeContext db)
	{
		lock (db)
		{
			Cache.LastUpdate = DateTime.Now;
		}
	}

	/// <summary>
	/// Сообщение аудита в БД
	/// </summary>
	/// <param name="db"></param>
	/// <param name="log"></param>
	/// <returns></returns>
	public static async Task LogAsync(
		this DatalakeContext db,
		Log log)
	{
		try
		{
			await db.InsertAsync(log);
		}
		catch { }
	}
}
