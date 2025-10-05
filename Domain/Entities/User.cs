using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Учетная запись
/// </summary>
public record class User : IWithGuidKey, ISoftDeletable
{
	private User() { }

	public User(UserType type, string? fullName, string? login, string? passwordString, Guid? energoIdGuid, string? host, bool generateNewHash = false)
	{
		Guid = Guid.NewGuid();

		Update(type, fullName, login, passwordString, energoIdGuid, host, generateNewHash);
	}

	public static User CreateFromStaticOptions(string name, string token, string? host)
	{
		return new User
		{
			Guid = Guid.NewGuid(),
			Type = UserType.Static,
			FullName = name,
			StaticHost = host,
			PasswordHash = PasswordHashValue.FromExistingHash(token),
		};
	}

	public static User CreateFromLoginPassword(string login, string password)
	{
		return new User
		{
			Guid = Guid.NewGuid(),
			Type = UserType.Local,
			Login = login,
			PasswordHash = PasswordHashValue.FromPlainText(password),
		};
	}

	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Учетная запись уже удалена");

		IsDeleted = true;
	}

	public void Update(UserType type, string? fullName, string? login, string? passwordString, Guid? energoIdGuid, string? host, bool generateNewHash = false)
	{
		if (type == UserType.Local)
			UpdateAsLocal(
				login: login,
				fullName: fullName,
				passwordString: passwordString);

		else if (type == UserType.Static)
			UpdateAsStatic(
				fullName: fullName,
				host: host,
				generateNewHash: generateNewHash);

		else if (type == UserType.EnergoId)
			UpdateAsEnergoId(
				fullName: fullName,
				energoIdGuid: energoIdGuid);

		else
			throw new DomainException("Указанный тип учетной записи не поддерживается: " + type);
	}

	private void UpdateAsLocal(string? login, string? fullName, string? passwordString)
	{
		if (string.IsNullOrEmpty(login))
			throw new DomainException("Логин не может быть пустым");

		Type = UserType.Local;
		Login = login;
		PasswordHash = PasswordHashValue.FromPlainText(passwordString);
		SetFullName(fullName);

		EnergoIdGuid = null;
		StaticHost = null;
	}

	private void UpdateAsEnergoId(Guid? energoIdGuid, string? fullName)
	{
		if (!energoIdGuid.HasValue)
			throw new DomainException("Идентификатор EnergoId не может быть пустым");

		Type = UserType.EnergoId;
		EnergoIdGuid = energoIdGuid.Value;
		SetFullName(fullName);

		Login = null;
		PasswordHash = null;
		StaticHost = null;
	}

	private void UpdateAsStatic(string? fullName, string? host, bool generateNewHash)
	{
		Type = UserType.Static;
		StaticHost = host;
		SetFullName(fullName);

		if (string.IsNullOrEmpty(PasswordHash?.Value) || generateNewHash)
			PasswordHash = PasswordHashValue.FromEmpty();

		Login = null;
		EnergoIdGuid = null;
	}

	private void SetFullName(string? fullName)
	{
		if (string.IsNullOrEmpty(fullName))
			throw new DomainException("Имя статичной учетной записи является обязательным");

		FullName = fullName;
	}

	/// <summary>
	/// Проверка пароля (опционально, можно делать через сервис)
	/// </summary>
	public bool VerifyPassword(string? plainText)
	{
		if (PasswordHash is null)
			return false;

		return PasswordHash.Verify(plainText);
	}

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Guid { get; private set; }

	/// <summary>
	/// Тип учетной записи
	/// </summary>
	public UserType Type { get; private set; }

	/// <summary>
	/// Учетная запись отмечена как удаленная
	/// </summary>
	public bool IsDeleted { get; private set; } = false;

	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; private set; }

	/// <summary>
	/// Полное имя
	/// </summary>
	public string FullName { get; private set; } = string.Empty;

	/// <summary>
	/// Хэш пароля
	/// </summary>
	public PasswordHashValue? PasswordHash { get; private set; }

	/// <summary>
	/// Адрес, с которого разрешен доступ
	/// </summary>
	public string? StaticHost { get; private set; }

	/// <summary>
	/// Идентификатор в EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; private set; }


	// связи

	/// <summary>
	/// Список связей с группами пользователей
	/// </summary>
	public ICollection<UserGroupRelation> GroupsRelations { get; set; } = [];

	/// <summary>
	/// Список групп пользователей
	/// </summary>
	public ICollection<UserGroup> Groups { get; set; } = [];

	/// <summary>
	/// Список правил доступа, выданных этой учетной записи
	/// </summary>
	public ICollection<AccessRights> AccessRules { get; set; } = [];

	/// <summary>
	/// Список действий пользователя, записанных в аудит
	/// </summary>
	public ICollection<Log> Actions { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<Log> Logs { get; set; } = null!;

	/// <summary>
	/// Информация о пользователе из EnergoId
	/// </summary>
	public EnergoId? EnergoId { get; set; }
}
