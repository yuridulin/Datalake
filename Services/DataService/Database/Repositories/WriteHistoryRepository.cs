using Datalake.DataService.Database.Constants;
using Datalake.DataService.Database.Entities;
using Datalake.DataService.Database.Interfaces;
using Datalake.PrivateApi.Attributes;
using LinqToDB.Data;

namespace Datalake.DataService.Database.Repositories;

[Scoped]
public class WriteHistoryRepository(
	DataLinqToDbContext db,
	ILogger<WriteHistoryRepository> logger) : IWriteHistoryRepository
{
	public async Task<bool> WriteAsync(IEnumerable<TagHistory> records)
	{
		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.ExecuteAsync(CreateTempTableForWrite);
			await db.BulkCopyAsync(BulkCopyOptions, records);
			await db.ExecuteAsync(WriteSql);

			await transaction.CommitAsync();
			return true;
		}
		catch (Exception e)
		{
			logger.LogError(e, "Не удалось записать данные");
			await transaction.RollbackAsync();
			return false;
		}
	}

	private static readonly BulkCopyOptions BulkCopyOptions = new() { TableName = TempTableForWrite, BulkCopyType = BulkCopyType.ProviderSpecific, };

	private const string TempTableForWrite = "TagsHistoryState";

	private const string CreateTempTableForWrite = $@"
		CREATE TEMPORARY TABLE ""{TempTableForWrite}"" (LIKE {DataDefinition.Schema}.""{DataDefinition.TagHistory.Table}"" EXCLUDING INDEXES)
		ON COMMIT DROP;";

	private const string WriteSql = $@"
		INSERT INTO {DataDefinition.Schema}.""{DataDefinition.TagHistory.Table}"" (
			""{DataDefinition.TagHistory.TagId}"",
			""{DataDefinition.TagHistory.Date}"",
			""{DataDefinition.TagHistory.Text}"",
			""{DataDefinition.TagHistory.Number}"",
			""{DataDefinition.TagHistory.Quality}""
		)
		SELECT
			""{DataDefinition.TagHistory.TagId}"",
			""{DataDefinition.TagHistory.Date}"",
			""{DataDefinition.TagHistory.Text}"",
			""{DataDefinition.TagHistory.Number}"",
			""{DataDefinition.TagHistory.Quality}""
		FROM ""{TempTableForWrite}""
		ON CONFLICT (""{DataDefinition.TagHistory.TagId}"", ""{DataDefinition.TagHistory.Date}"")
		DO UPDATE SET
			""{DataDefinition.TagHistory.Text}""   = EXCLUDED.""{DataDefinition.TagHistory.Text}"",
			""{DataDefinition.TagHistory.Number}"" = EXCLUDED.""{DataDefinition.TagHistory.Number}"",
			""{DataDefinition.TagHistory.Quality}""= EXCLUDED.""{DataDefinition.TagHistory.Quality}"";";
}