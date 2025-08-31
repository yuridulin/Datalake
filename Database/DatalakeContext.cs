using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.InMemory.Stores.Derived;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.Database.Views;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
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
	static DatalakeContext()
	{
		Configuration.Linq.GuardGrouping = false;
	}

	/// <summary>
	/// Создание или изменение заранее определенных статичных учетных записей.
	/// Выполняется до начала работы стора.
	/// </summary>
	/// <param name="users">Список необходимых данных о учетных записях</param>
	public async Task EnsureStaticUsersAsync((string Login, string Token, AccessType AccessType, string? Host)[] users)
	{
		using var transaction = await BeginTransactionAsync();

		foreach (var user in users)
		{
			var existUser = await Users.FirstOrDefaultAsync(x => x.Login == user.Login && x.Type == UserType.Static)
				?? await Users
					.Value(x => x.Guid, Guid.NewGuid())
					.Value(x => x.Type, UserType.Static)
					.Value(x => x.Login, user.Login)
					.Value(x => x.FullName, user.Login)
					.Value(x => x.PasswordHash, user.Token)
					.Value(x => x.StaticHost, user.Host)
					.InsertWithOutputAsync();

			var existRule = await AccessRights.FirstOrDefaultAsync(x => x.UserGuid == existUser.Guid && x.IsGlobal)
				?? await AccessRights
					.Value(x => x.UserGuid, existUser.Guid)
					.Value(x => x.IsGlobal, true)
					.Value(x => x.AccessType, user.AccessType)
					.InsertWithOutputAsync();

			await Users
				.Where(x => x.Guid == existUser.Guid)
				.Set(x => x.PasswordHash, user.Token)
				.Set(x => x.StaticHost, user.Host)
				.UpdateAsync();

			await AccessRights
				.Where(x => x.Id == existRule.Id)
				.Set(x => x.AccessType, user.AccessType)
				.UpdateAsync();
		}

		await transaction.CommitAsync();
	}

	/// <summary>
	/// Необходимые для работы записи, которые должны быть в базе данных
	/// </summary>
	public async Task EnsureDataCreatedAsync(
		UsersMemoryRepository usersRepository,
		DatalakeSessionsStore sessionsStore)
	{
		// запись необходимых источников в список
		var customSources = Lists.CustomSources
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
			await usersRepository.ProtectedCreateAsync(this, Guid.Empty, Defaults.InitialAdmin);
		}

		// Загрузка сессий пользователей
		await sessionsStore.InitializeAsync();

		await AuditRepository.WriteAsync(
			this,
			"Сервер запущен",
			category: LogCategory.Core,
			type: LogType.Success
		);
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

	internal ITable<EnergoIdUserView> UsersEnergoId
		=> this.GetTable<EnergoIdUserView>();

	internal ITable<UserSession> UserSessions
		=> this.GetTable<UserSession>();

	#endregion
}
