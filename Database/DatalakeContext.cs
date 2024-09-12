using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Tags;
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

		// создание администратора по умолчанию, если его учетки нет
		if (!Users.Any(x => x.Login == "admin"))
		{
			await new UsersRepository(this).CreateAsync(Defaults.InitialAdmin);
		}

		// заполнение кэша
		var schemaProvider = DataProvider.GetSchemaProvider();
		var schema = schemaProvider.GetSchema(this);

		Cache.Tables.AddRange([.. schema.Tables
			.Where(x => x.TableName!.StartsWith(ValuesRepository.NamePrefix))
			.Select(x => x.TableName!)
			.OrderByDescending(x => x)]);

		Cache.Tags = await (
			from t in Tags
			from s in Sources.LeftJoin(x => x.Id == t.SourceId)
			select new TagCacheInfo
			{
				Id = t.Id,
				Guid = t.GlobalGuid,
				TagType = t.Type,
				SourceType = s.Type,
			}
		).ToDictionaryAsync(x => x.Id, x => x);

		// актуализация таблицы текущих значений
		var lastTable = Cache.Tables.LastOrDefault();
		if (lastTable != null)
		{
			var lastValues = await this.GetTable<TagHistory>().TableName(lastTable)
				.GroupBy(x => x.TagId)
				.Select(g => g.OrderByDescending(x => x.Date).First())
				.ToArrayAsync();

			Cache.LiveValuesSet(lastValues);
		}
		var notExistedValues = Cache.Tags.Keys
			.Where(x => !Cache.LiveValues.ContainsKey(x))
			.Select(x => new TagHistory
			{
				TagId = x,
				Date = DateTime.Today,
				Number = null,
				Text = null,
				Quality = TagQuality.Bad_NoValues,
				Using = TagUsing.NotFound,
			})
			.ToArray();
		if (notExistedValues.Length != 0)
		{
			Cache.LiveValuesSet(notExistedValues);
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
