using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using Datalake.Database.Repositories;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using System.Linq;

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

		var realHistoryTables = schema.Tables
			.Where(x => x.TableName != null)
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

		if (realHistoryTables.Length > 0)
		{
			await TagHistoryChunks.DeleteAsync();
			await TagHistoryChunks.BulkCopyAsync(realHistoryTables);
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

		// создание администратора по умолчанию, если его учетки нет
		if (!Users.Any(x => x.Login == "admin"))
		{
			await new UsersRepository(this).CreateAsync(Defaults.InitialAdmin);
		}

		// актуализация таблицы текущих значенийD
		var tagsWithoutLiveValues = await (
			from tag in Tags
			from live in TagsLive.LeftJoin(x => x.TagId == tag.Id)
			where live == null
			select tag.Id
		).ToArrayAsync();

		if (tagsWithoutLiveValues.Length > 0)
		{
			// пробуем прочитать последние значения из последнего чанка
			if (realHistoryTables.Length > 0)
			{
				var tags = await Tags.Select(x => x.Id).ToArrayAsync();
				var lastHistoryChunk = realHistoryTables.OrderByDescending(x => x.Date).Select(x => x.Table).First();
				var lastHistoryTable = this.GetTable<TagHistory>().TableName(lastHistoryChunk);

				var lastHistoryValues = await lastHistoryTable
					.Where(x => tagsWithoutLiveValues.Contains(x.TagId))
					.GroupBy(x => x.TagId)
					.Select(g => g.OrderByDescending(x => x.Date).First())
					.ToArrayAsync();

				await TagsLive.BulkCopyAsync(lastHistoryValues);

				// после вставки значений урезаем список несуществующих текущих
				tagsWithoutLiveValues = tagsWithoutLiveValues.Except(lastHistoryValues.Select(x => x.TagId)).ToArray();
			}

			// создаем заглушки для отсутствующих в текущих тегов
			await TagsLive.BulkCopyAsync(tagsWithoutLiveValues.Select(x => new TagHistory
			{
				TagId = x,
				Date = DateTime.Today,
				Number = null,
				Text = null,
				Quality = TagQuality.Bad_NoValues,
				Using = TagUsing.NotFound,
			}));
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
