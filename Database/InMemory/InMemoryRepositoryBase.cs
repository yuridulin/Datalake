using Microsoft.Extensions.DependencyInjection;

namespace Datalake.Database.InMemory;

/// <summary>
/// Базовый репозиторий для работы с объектами БД в памяти
/// </summary>
public abstract class InMemoryRepositoryBase
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	/// <param name="inMemory">Общий контекст данных в памяти</param>
	public InMemoryRepositoryBase(
		IServiceScopeFactory serviceScopeFactory,
		Lazy<InMemoryRepositoriesManager> inMemory)
	{
		_inMemory = inMemory;

		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabaseAndTrigger(db).Wait();
	}

	private readonly Lazy<InMemoryRepositoriesManager> _inMemory;

	/// <summary>
	/// Общий контекст данных в памяти
	/// </summary>
	protected InMemoryRepositoriesManager InMemory => _inMemory.Value;

	/// <summary>
	/// Событие изменения
	/// </summary>
	public event EventHandler<int>? Updated;

	/// <summary>
	/// Вызов события изменения
	/// </summary>
	public void Trigger()
	{
		UpdateVersion(DateTime.UtcNow.Ticks.ToString());
		Updated?.Invoke(this, 0);
	}


	#region Работа с БД

	/// <summary>
	/// Обновление данных из БД
	/// </summary>
	/// <param name="db"></param>
	/// <returns></returns>
	private async Task InitializeFromDatabaseAndTrigger(DatalakeContext db)
	{
		await InitializeFromDatabase(db);

		UpdateVersion(DateTime.UtcNow.Ticks.ToString());
		Updated?.Invoke(this, 0);
	}

	/// <summary>
	/// Обновление данных из БД
	/// </summary>
	/// <param name="db"></param>
	/// <returns></returns>
	protected abstract Task InitializeFromDatabase(DatalakeContext db);

	/// <summary>
	/// Обновление данных из БД
	/// </summary>
	/// <param name="db">Контекст БД</param>
	public virtual async Task RefreshFromDatabase(DatalakeContext db)
	{
		await InitializeFromDatabaseAndTrigger(db);
	}

	#endregion


	#region Версионность

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Получение текущей версии данных репозитория
	/// </summary>
	public string CurrentVersion
	{
		get { lock (_versionLock) return _globalVersion; }
	}

	/// <summary>
	/// Установка новой версии данных репозитория
	/// </summary>
	/// <param name="newVersion">Значение новой версии</param>
	public void UpdateVersion(string newVersion)
	{
		lock (_versionLock)
			_globalVersion = newVersion;
	}

	#endregion
}
