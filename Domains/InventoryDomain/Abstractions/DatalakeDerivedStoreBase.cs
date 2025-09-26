using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Stores;
using Datalake.PrivateApi.Utils;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.Abstractions;

/// <summary>
/// Базовый класс дочернего стора
/// </summary>
/// <typeparam name="TState">Тип зависимых данных</typeparam>
public abstract class DatalakeDerivedStoreBase<TState>
{
	/// <summary>БАЗА</summary>
	public DatalakeDerivedStoreBase(
		DatalakeDataStore dataStore,
		ILogger logger)
	{
		Logger = logger;

		// Вычисляем изначальное состояние сразу же
		lock (this)
		{
			_lastProcessingDataVersion = dataStore.State.Version;
			_state = Measures.Measure(() => CreateDerivedState(dataStore.State), Logger, $"{nameof(CreateDerivedState)}<{Type}>");
			Logger.LogInformation("Инициализация зависимых данных {Type} для версии {Version}", Type, _lastProcessingDataVersion);
		}

		// Подписываемся на будущие изменения
		dataStore.StateChanged += (_, newState) =>
		{
			// Атомарно проверяем и обновляем версию
			long currentVersion = Interlocked.Read(ref _lastProcessingDataVersion);
			if (newState.Version > currentVersion)
			{
				// Пытаемся установить новую версию
				long originalVersion = Interlocked.CompareExchange(
					ref _lastProcessingDataVersion,
					newState.Version,
					currentVersion);

				// Если удалось установить - запускаем обработку
				if (originalVersion == currentVersion)
				{
					Task.Run(() => Rebuild(newState));
				}
			}
		};
	}

	/// <summary>
	/// Текущее состояние
	/// </summary>
	public TState State => _state;

	/// <summary>
	/// Событие при изменении состояния
	/// </summary>
	public event EventHandler<TState>? StateChanged;

	/// <summary>
	/// Вычисление нового состояния
	/// </summary>
	protected abstract TState CreateDerivedState(DatalakeDataState newState);

	/// <summary>
	/// Логгер
	/// </summary>
	protected readonly ILogger Logger;

	private TState _state;
	private long _lastProcessingDataVersion;

	/// <summary>
	/// Описание типа данных
	/// </summary>
	protected abstract string Type { get; }

	private void Rebuild(DatalakeDataState newDataState)
	{
		try
		{
			// Вычисляем новые права доступа
			var newState = Measures.Measure(() => CreateDerivedState(newDataState), Logger, $"{nameof(CreateDerivedState)}<{Type}>");

			// Атомарно обновляем состояние
			Interlocked.Exchange(ref _state, newState);

			// Убеждаемся, что мы обработали самую свежую версию
			long currentVersion = Interlocked.Read(ref _lastProcessingDataVersion);
			if (newDataState.Version >= currentVersion)
			{
				// Вызываем событие только для самой актуальной версии
				Task.Run(() => StateChanged?.Invoke(this, newState));
			}

			Logger.LogInformation("Завершено обновление зависимых данных {Type} для версии {Version}", Type, newDataState.Version);
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Не удалось обновить зависимые данные {Type} для версии {Version}", Type, newDataState.Version);
		}
	}
}
