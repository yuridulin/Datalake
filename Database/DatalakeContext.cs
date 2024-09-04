using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using Datalake.Database.Repositories;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;

namespace Datalake.Database;

public class DatalakeContext(DataOptions<DatalakeContext> options) : DataConnection(options.Options)
{
	public static void SetupLinqToDB()
	{
		Configuration.Linq.GuardGrouping = false;
	}

	/// <summary>
	/// Необходимые для работы записи, которые должны быть в базе данных
	/// </summary>
	public async Task EnsureDataCreatedAsync()
	{
		// заполнение списка партицированных таблиц по реальной базе
		var schemaProvider = DataProvider.GetSchemaProvider();
		var schema = schemaProvider.GetSchema(this);

		var storedHistoryTables = await TagHistoryChunks.Select(x => x.Table).ToArrayAsync();
		var notStoredHistoryTables = schema.Tables
			.Where(x => x.TableName != null)
			.Where(x => !storedHistoryTables.Contains(x.TableName))
			.Where(x => x.TableName!.StartsWith(ValuesRepository.NamePrefix))
			.Select(x => new TagHistoryChunk
			{
				Table = x.TableName!,
				Date = DateOnly.TryParseExact(
					x.TableName!.Replace(ValuesRepository.NamePrefix, ""),
					ValuesRepository.DateMask,
					out var date) ? date : DateOnly.MinValue,
			})
			.Where(x => x.Date != DateOnly.MinValue)
			.ToArray();

		if (notStoredHistoryTables.Length > 0)
		{
			await this.BulkCopyAsync(notStoredHistoryTables);
		}

		// запись необходимых источников в список
		var customSources = Enum.GetValues<CustomSource>()
			.Select(x => new Source
			{
				Id = (int)x,
				Name = x.ToString(),
				Description = x.GetDescription(),
				Type = SourceType.Custom,
			})
			.ToArray();

		foreach (var source in customSources)
		{
			if (!await Sources.AnyAsync(x => x.Id == source.Id))
			{
				int count = await Sources
					.Value(x => x.Id, source.Id)
					.Value(x => x.Name, source.Name)
					.Value(x => x.Description, source.Description)
					.Value(x => x.Type, source.Type)
					.InsertAsync();

				if (count == 0)
					throw new Exception("Не удалось добавить источник по умолчанию: " + (CustomSource)source.Id);
			}
		}

		// создание таблицы настроек
		if (!await Settings.AnyAsync())
		{
			var setting = new Settings();
			int count = await this.InsertAsync(setting);

			if (count == 0)
				throw new Exception("Не удалось создать строку настроек");
		}
		else
		{
			await this.SetLastUpdateToNowAsync();
		}

		// создание пользователя по умолчанию
		if (!Users.Any())
		{
			await new UsersRepository(this).CreateAsync(Defaults.InitialAdmin);
		}
	}

	#region Таблицы

	public ITable<AccessRights> AccessRights
		=> this.GetTable<AccessRights>();

	public ITable<Block> Blocks
		=> this.GetTable<Block>();

	public ITable<BlockProperty> BlockProperties
		=> this.GetTable<BlockProperty>();

	public ITable<BlockTag> BlockTags
		=> this.GetTable<BlockTag>();

	public ITable<Log> Logs
		=> this.GetTable<Log>();

	public ITable<Settings> Settings
		=> this.GetTable<Settings>();

	public ITable<Source> Sources
		=> this.GetTable<Source>();

	public ITable<Tag> Tags
		=> this.GetTable<Tag>();

	public ITable<TagHistoryChunk> TagHistoryChunks
		=> this.GetTable<TagHistoryChunk>();

	public ITable<TagInput> TagInputs
		=> this.GetTable<TagInput>();

	public ITable<TagHistory> TagsLive
		=> this.GetTable<TagHistory>();

	public ITable<User> Users
		=> this.GetTable<User>();

	public ITable<UserGroup> UserGroups
		=> this.GetTable<UserGroup>();

	public ITable<UserGroupRelation> UserGroupRelations
		=> this.GetTable<UserGroupRelation>();

	#endregion
}
