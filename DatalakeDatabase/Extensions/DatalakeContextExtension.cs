using DatalakeDatabase.Models;
using LinqToDB;

namespace DatalakeDatabase.Extensions;

public static class DatalakeContextExtension
{
	/// <summary>
	/// Обновление времени последнего изменения структуры тегов, источников и сущностей в базе данных
	/// </summary>
	/// <param name="db">Подключение к базе данных</param>
	public static async Task UpdateAsync(this DatalakeContext db)
	{
		await db.Settings
			.Set(x => x.LastUpdate, DateTime.Now)
			.UpdateAsync();
	}

	public static async Task<DateTime> GetLastUpdateAsync(this DatalakeContext db)
	{
		var lastUpdate = await db.Settings
			.Select(x => x.LastUpdate)
			.DefaultIfEmpty(DateTime.MinValue)
			.FirstOrDefaultAsync();

		return lastUpdate;
	}

	public static async Task LogAsync(this DatalakeContext db, Log log)
	{
		try
		{
			await db.InsertAsync(log);
		}
		catch { }
	}
}
