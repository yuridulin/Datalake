using Datalake.Database.Extensions;
using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Stores.Derived;

/// <summary>
/// Репозиторий для работы с сессиями пользователей
/// </summary>
public class DatalakeSessionsStore
{
	/// <summary>
	/// Активные сессии
	/// </summary>
	public ConcurrentDictionary<string, UserSessionInfo> Sessions { get; set; } = [];

	/// <summary>
	/// Сессии статичных учетных записей
	/// </summary>
	public ConcurrentDictionary<string, UserSessionInfo> StaticSessions { get; set; } = [];

	/// <summary>Конструктор</summary>
	public DatalakeSessionsStore(
		DatalakeDataStore dataStore,
		DatalakeAccessStore accessStore,
		IServiceScopeFactory serviceScopeFactory,
		ILogger<DatalakeSessionsStore> logger)
	{
		this.dataStore = dataStore;
		this.accessStore = accessStore;
		this.serviceScopeFactory = serviceScopeFactory;
		this.logger = logger;

		dataStore.StateChanged += (o, state) =>
		{
			Task.Run(() => ReloadStaticSessions(state));
		};

		// Так как загрузка сохраненных сессий должна быть еще до запуска, это сделает инициализатор БД
	}

	/// <summary>
	/// Получить текущую сессию по токену
	/// </summary>
	/// <param name="token">Токен сессии</param>
	/// <param name="address">Адрес, с которого разрешен доступ по статичной учетной записи</param>
	/// <returns>Информация о сессии</returns>
	public async Task<UserSessionInfo?> GetExistSessionAsync(string token, string address)
	{
		if (!Sessions.TryGetValue(token, out var session))
			if (!StaticSessions.TryGetValue(token, out session))
				return null;

		if (!string.IsNullOrWhiteSpace(session.StaticHost) && session.StaticHost != address)
			return null;

		if (session.ExpirationTime < DateTime.UtcNow)
		{
			await RemoveSession(session);
			return null;
		}

		if (accessStore.State.TryGet(session.UserGuid, out var userRights))
			session.AuthInfo = userRights;

		return session;
	}

	/// <summary>
	/// Получить текущую сессию по токену
	/// </summary>
	/// <param name="context">Контекст запроса</param>
	/// <returns>Информация о сессии, если она есть</returns>
	public async Task<UserSessionInfo?> GetExistSessionAsync(HttpContext context)
	{
		var token = context.Request.Headers[AuthConstants.TokenHeader].ToString();

		if (string.IsNullOrEmpty(token))
			return null;

		var address = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

		var session = await GetExistSessionAsync(token, address);
		if (session == null)
			return null;

		context.Response.AddSessionToResponse(session);
		return session;
	}

	/// <summary>
	/// Создание новой сессии для пользователя (не статичного, у них свои сессии)
	/// </summary>
	/// <param name="userAuthInfo">Информация о пользователе</param>
	/// <param name="type">Тип входа</param>
	/// <returns>Информация о сессии</returns>
	public async Task<UserSessionInfo> OpenSessionAsync(UserAuthInfo userAuthInfo, UserType type)
	{
		var session = new UserSessionInfo
		{
			UserGuid = userAuthInfo.Guid,
			AuthInfo = userAuthInfo,
			Token = new Random().Next().ToString(),
			ExpirationTime = DateTime.UtcNow.AddDays(7), // срок жизни сессии
			StaticHost = string.Empty,
			Type = type,
		};

		Sessions.AddOrUpdate(session.Token, (token) => session, (token, exist) => session);

		// сохранение в БД
		using var scope = serviceScopeFactory.CreateScope();
		using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		await db.UserSessions
			.Value(x => x.UserGuid, session.UserGuid)
			.Value(x => x.Token, session.Token)
			.Value(x => x.Type, session.Type)
			.Value(x => x.ExpirationTime, session.ExpirationTime)
			.Value(x => x.Created, DateFormats.GetCurrentDateTime())
			.InsertAsync();

		return session;
	}

	/// <summary>
	/// Закрытие сессии
	/// </summary>
	/// <param name="token">Токен сессии</param>
	public async Task CloseSessionAsync(string token)
	{
		var session = await GetExistSessionAsync(token, string.Empty);
		if (session != null)
		{
			await RemoveSession(session);
		}
	}

	/// <summary>
	/// Инициализация сессий при запуске
	/// </summary>
	internal async Task InitializeAsync()
	{
		await LoadStoredSessionsAsync();
		ReloadStaticSessions(dataStore.State);
	}

	private readonly DatalakeDataStore dataStore;
	private readonly DatalakeAccessStore accessStore;
	private readonly IServiceScopeFactory serviceScopeFactory;
	private readonly ILogger<DatalakeSessionsStore> logger;
	private readonly Lock _initLock = new();

	/// <summary>
	/// Чтение сохраненных сессий из БД
	/// </summary>
	/// <returns></returns>
	private async Task LoadStoredSessionsAsync()
	{
		// получаем список актуальных сессий из БД
		// при этом мы удаляем неактуальные (просроченные) сессии
		using var scope = serviceScopeFactory.CreateScope();
		using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		await db.UserSessions
			.Where(x => x.ExpirationTime <= DateFormats.GetCurrentDateTime())
			.DeleteAsync();

		var storedSessions = await db.UserSessions
			.Select(x => new UserSessionInfo
			{
				UserGuid = x.UserGuid,
				ExpirationTime = x.ExpirationTime,
				Token = x.Token,
				Type = x.Type,
			})
			.ToDictionaryAsync(x => x.Token);

		lock (_initLock)
		{
			Sessions = new(storedSessions);
		}
	}

	/// <summary>
	/// Обновление сессий для статичных учетных записей
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	private void ReloadStaticSessions(DatalakeDataState state)
	{
		try
		{
			var staticUsers = state.Users
				.Where(x => x.Type == UserType.Static)
				.ToList();

			var sessions = staticUsers
				.Select(user => new UserSessionInfo
				{
					ExpirationTime = DateTime.MaxValue,
					UserGuid = user.Guid,
					Token = user.PasswordHash ?? string.Empty,
					StaticHost = user.StaticHost,
					Type = user.Type,
				})
				.ToDictionary(x => x.Token);

			lock (_initLock)
			{
				StaticSessions = new(sessions);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка обновления пользователей");
		}
	}

	/// <summary>
	/// Удаление сессии из списка
	/// </summary>
	/// <param name="session">Выбранная сессия</param>
	private async Task RemoveSession(UserSessionInfo session)
	{
		if (Sessions.TryRemove(session.Token, out _))
		{
			// удаление из БД
			using var scope = serviceScopeFactory.CreateScope();
			using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

			await db.UserSessions
				.Where(x => x.Token == session.Token)
				.DeleteAsync();
		}
	}
}
