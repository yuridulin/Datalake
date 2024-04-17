using LinqToDB;

namespace DatalakeDatabase.Extensions;

public static class DatalakeContextExtension
{
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
}
