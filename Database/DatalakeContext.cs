using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Tags;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using Datalake.Database.Repositories;
using Datalake.Database.Utilities;
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

		var existsCustomSources = await Sources
			.Where(x => customSources.Select(c => c.Id).Contains(x.Id))
			.Select(x => x.Id)
			.ToArrayAsync();
		
		await Sources.BulkCopyAsync(
			new BulkCopyOptions { KeepIdentity = true },
			customSources.ExceptBy(existsCustomSources, x => x.Id));

		// создание таблицы настроек
		if (!await Settings.AnyAsync())
		{
			var setting = new Settings();
			int count = await this.InsertAsync(setting);

			if (count == 0)
				throw new Exception("Не удалось создать строку настроек");
		}

		// создание администратора по умолчанию, если его учетки нет
		if (!Users.Any(x => x.Login == "admin"))
		{
			await new UsersRepository(this).CreateAsync(Defaults.InitialAdmin);
		}

		// заполнение кэша
		var valuesRepository = new ValuesRepository(this);

		var tables = await valuesRepository.GetHistoryTablesFromSchema();

		Cache.Tables = tables
			.Where(x => x.Name.StartsWith(ValuesRepository.NamePrefix))
			.Select(x => new
			{
				Date = ValuesRepository.GetTableDate(x.Name),
				x.Name,
			})
			.Where(x => x.Date != DateTime.MinValue)
			.DistinctBy(x => x.Date)
			.ToDictionary(x => x.Date, x => x.Name);

		Cache.Tags = await (
			from t in Tags
			from s in Sources.LeftJoin(x => x.Id == t.SourceId)
			select new TagCacheInfo
			{
				Id = t.Id,
				Guid = t.GlobalGuid,
				Name = t.Name,
				TagType = t.Type,
				SourceType = s.Type,
				IsManual = t.SourceId == (int)CustomSource.Manual,
				ScalingCoefficient = t.IsScaling 
					? ((t.MaxEu - t.MinEu) / (t.MaxRaw - t.MinRaw))
					: 1,
			}
		).ToDictionaryAsync(x => x.Id, x => x);

		// создание таблицы для значений на текущую дату
		if (!Cache.Tables.ContainsKey(DateTime.Today))
		{
			await valuesRepository.GetHistoryTableAsync(DateTime.Today);
		}

		// актуализация таблицы текущих значений
		var lastValues = await valuesRepository.ReadHistoryValuesAsync([.. Cache.Tags.Keys], DateTime.Now, DateTime.Now);

		Live.Write(lastValues);
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

	public ITable<TagInput> TagInputs
		=> this.GetTable<TagInput>();

	public ITable<User> Users
		=> this.GetTable<User>();

	public ITable<UserGroup> UserGroups
		=> this.GetTable<UserGroup>();

	public ITable<UserGroupRelation> UserGroupRelations
		=> this.GetTable<UserGroupRelation>();

	#endregion
}
