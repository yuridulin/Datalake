using Datalake.Database.Extensions;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;

namespace Datalake.Database;

/// <summary>
/// Контекст для работы с базой данных
/// </summary>
public class DatalakeContext(DataOptions<DatalakeContext> options) : DataConnection(options.Options)
{
	/// <summary>
	/// Специфичные настройки LinqToDB, которые необходимо применять при запуске
	/// </summary>
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
		var customSources = SourcesRepository.CustomSourcesId
			.Select(x => new Source
			{
				Id = (int)x,
				Name = x.ToString(),
				Description = x.GetDescription(),
				Type = x,
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
			await UsersRepository.CreateAsync(this, Guid.Empty, Defaults.InitialAdmin);
		}

		// заполнение кэша
		await SystemRepository.RebuildStorageCacheAsync(this);
		await AccessRepository.RebuildUserRightsCacheAsync(this);
		SystemRepository.Update();
		AccessRepository.Update();
	}

	#region Таблицы

	internal ITable<AccessRights> AccessRights
		=> this.GetTable<AccessRights>();

	internal ITable<Block> Blocks
		=> this.GetTable<Block>();

	internal ITable<BlockProperty> BlockProperties
		=> this.GetTable<BlockProperty>();

	internal ITable<BlockTag> BlockTags
		=> this.GetTable<BlockTag>();

	internal ITable<Log> Logs
		=> this.GetTable<Log>();

	internal ITable<Settings> Settings
		=> this.GetTable<Settings>();

	internal ITable<Source> Sources
		=> this.GetTable<Source>();

	internal ITable<Tag> Tags
		=> this.GetTable<Tag>();

	internal ITable<TagInput> TagInputs
		=> this.GetTable<TagInput>();

	internal ITable<TagHistory> TagsHistory
		=> this.GetTable<TagHistory>();

	internal ITable<User> Users
		=> this.GetTable<User>();

	internal ITable<UserGroup> UserGroups
		=> this.GetTable<UserGroup>();

	internal ITable<UserGroupRelation> UserGroupRelations
		=> this.GetTable<UserGroupRelation>();

	#endregion
}
