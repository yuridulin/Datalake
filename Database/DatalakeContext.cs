using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Extensions;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;

namespace Datalake.Database;

/// <summary>
/// Контекст для работы с базой данных
/// </summary>
public class DatalakeContext : DataConnection
{
	/// <summary>
	/// Конструктор
	/// </summary>
	public DatalakeContext(DataOptions<DatalakeContext> options) : base(options.Options)
	{
		_accessRepository = new Lazy<AccessRepository>(() => new AccessRepository(this));
		_blocksRepository = new Lazy<BlocksRepository>(() => new BlocksRepository(this));
		_sourcesRepository = new Lazy<SourcesRepository>(() => new SourcesRepository(this));
		_systemRepository = new Lazy<SystemRepository>(() => new SystemRepository(this));
		_tagsRepository = new Lazy<TagsRepository>(() => new TagsRepository(this));
		_usersRepository = new Lazy<UsersRepository>(() => new UsersRepository(this));
		_userGroupsRepository = new Lazy<UserGroupsRepository>(() => new UserGroupsRepository(this));
		_valuesRepository = new Lazy<ValuesRepository>(() => new ValuesRepository(this));
		_tablesRepository = new Lazy<TablesRepository>(() => new TablesRepository(this));
	}

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
			await UsersRepository.CreateAsync(Defaults.InitialAdmin);
		}

		// заполнение кэша
		await SystemRepository.RebuildStorageCacheAsync();
		await AccessRepository.RebuildUserRightsCacheAsync();
	}

	#region Репозитории

	/// <summary>
	/// Репозиторий для работы с правами доступа
	/// </summary>
	public AccessRepository AccessRepository => _accessRepository.Value;
	private readonly Lazy<AccessRepository> _accessRepository;

	/// <summary>
	/// Репозиторий для работы с блоками тегов
	/// </summary>
	public BlocksRepository BlocksRepository => _blocksRepository.Value;
	private readonly Lazy<BlocksRepository> _blocksRepository;

	/// <summary>
	/// Репозиторий для работы с источниками данных
	/// </summary>
	public SourcesRepository SourcesRepository => _sourcesRepository.Value;
	private readonly Lazy<SourcesRepository> _sourcesRepository;

	/// <summary>
	/// Репозиторий для работы с настройками и системной информацией
	/// </summary>
	public SystemRepository SystemRepository => _systemRepository.Value;
	private readonly Lazy<SystemRepository> _systemRepository;

	/// <summary>
	/// Репозиторий для работы с тегами
	/// </summary>
	public TagsRepository TagsRepository => _tagsRepository.Value;
	private readonly Lazy<TagsRepository> _tagsRepository;

	/// <summary>
	/// Репозиторий для работы с пользователями
	/// </summary>
	public UsersRepository UsersRepository => _usersRepository.Value;
	private readonly Lazy<UsersRepository> _usersRepository;

	/// <summary>
	/// Репозиторий для работы с группами пользователей
	/// </summary>
	public UserGroupsRepository UserGroupsRepository => _userGroupsRepository.Value;
	private readonly Lazy<UserGroupsRepository> _userGroupsRepository;

	/// <summary>
	/// Репозиторий для работы со значениями
	/// </summary>
	public ValuesRepository ValuesRepository => _valuesRepository.Value;
	private readonly Lazy<ValuesRepository> _valuesRepository;

	/// <summary>
	/// Репозиторий для работы с таблицами с историей значений
	/// </summary>
	public TablesRepository TablesRepository => _tablesRepository.Value;
	private readonly Lazy<TablesRepository> _tablesRepository;

	#endregion

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

	internal ITable<User> Users
		=> this.GetTable<User>();

	internal ITable<UserGroup> UserGroups
		=> this.GetTable<UserGroup>();

	internal ITable<UserGroupRelation> UserGroupRelations
		=> this.GetTable<UserGroupRelation>();

	#endregion
}
