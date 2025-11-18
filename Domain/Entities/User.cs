using Datalake.Domain.Enums;
using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Учетная запись
/// </summary>
public record class User : IWithGuidKey, ISoftDeletable
{
	#region Конструкторы

	private User() { }

	/// <summary>
	/// Создание на основе указанных данных с выбором типа
	/// </summary>
	/// <param name="type">Тип учетной записи</param>
	/// <param name="energoIdGuid">Идентификатор учетной записи EnergoId</param>
	/// <param name="login">Логин</param>
	/// <param name="passwordString">Строка пароля</param>
	/// <param name="fullName">Полное имя</param>
	/// <param name="email">Почтовый адрес</param>
	/// <exception cref="DomainException">Тип не поддерживается</exception>
	public static User CreateWithType(UserType type, Guid? energoIdGuid, string? login, string? passwordString, string? fullName, string? email)
	{
		return type switch
		{
			UserType.Local => CreateFromLoginPassword(login, passwordString, fullName),
			UserType.EnergoId => CreateFromEnergoId(energoIdGuid, email, fullName),
			_ => throw new DomainException("Указанный тип учетной записи не поддерживается: " + type),
		};
	}

	/// <summary>
	/// Создание из данных EnergoId
	/// </summary>
	/// <param name="energoIdGuid">Идентификатор учетной записи EnergoId</param>
	/// <param name="email">Почтовый адрес</param>
	/// <param name="fullName">Полное имя</param>
	public static User CreateFromEnergoId(Guid? energoIdGuid, string? email, string? fullName)
	{
		if (energoIdGuid == null)
			throw new DomainException("Идентификатор EnergoId является обязательным для учетной записи с таким типом");

		var user = new User
		{
			Guid = energoIdGuid.Value,
		};
		user.UpdateAsEnergoId(fullName, email);
		return user;
	}

	/// <summary>
	/// Создание из логин-пароля
	/// </summary>
	/// <param name="login">Логин</param>
	/// <param name="passwordString">Строка пароля</param>
	/// <param name="fullName">Полное имя</param>
	public static User CreateFromLoginPassword(string? login, string? passwordString, string? fullName)
	{
		var user = new User
		{
			Guid = Guid.NewGuid(),
		};
		user.UpdateAsLocal(login, passwordString, fullName, null);
		return user;
	}

	#endregion Конструкторы

	#region Методы

	/// <inheritdoc/>
	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Учетная запись уже удалена");

		IsDeleted = true;
	}

	/// <summary>
	/// Изменение учетной записи
	/// </summary>
	/// <param name="login">Логин</param>
	/// <param name="passwordString">Строка пароля</param>
	/// <param name="fullName">Полное имя</param>
	/// <param name="email">Почтовый адрес</param>
	/// <exception cref="DomainException">Тип не поддерживается</exception>
	public void Update(string? login, string? passwordString, string? email, string? fullName)
	{
		if (Type == UserType.Local)
		{
			UpdateAsLocal(login, passwordString, fullName, email);
		}
		else if (Type == UserType.EnergoId)
		{
			UpdateAsEnergoId(fullName, email);
		}
		else
			throw new DomainException("Указанный тип учетной записи не поддерживается: " + Type);
	}

	private void UpdateAsLocal(string? login, string? passwordString, string? fullName, string? email)
	{
		if (string.IsNullOrEmpty(login))
			throw new DomainException("Логин не может быть пустым");

		if (string.IsNullOrEmpty(passwordString))
			throw new DomainException("Пароль не может быть пустым");

		Type = UserType.Local;
		Login = login;
		PasswordHash = PasswordHashValue.FromPlainText(passwordString);
		Email = email;
		FullName = fullName;
	}

	private void UpdateAsEnergoId(string? fullName, string? email)
	{
		Type = UserType.EnergoId;
		Login = null;
		PasswordHash = null;
		Email = email;
		FullName = fullName;
	}

	#endregion Методы

	#region Свойства

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
	/// Адрес электронной почты
	/// </summary>
	public string? Email { get; private set; }

	/// <summary>
	/// Имя для входа для локальных пользователей
	/// </summary>
	public string? Login { get; private set; }

	/// <summary>
	/// Хэш пароля для локальных пользователей
	/// </summary>
	public PasswordHashValue? PasswordHash { get; private set; }

	/// <summary>
	/// Полное имя для локальных пользователей
	/// </summary>
	public string? FullName { get; private set; }

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Список связей с группами пользователей
	/// </summary>
	public ICollection<UserGroupRelation> GroupsRelations { get; set; } = [];

	/// <summary>
	/// Список групп пользователей
	/// </summary>
	public ICollection<UserGroup> Groups { get; set; } = [];

	/// <summary>
	/// Информация о пользователе из EnergoId
	/// </summary>
	public EnergoId? EnergoId { get; set; }

	/// <summary>
	/// Список прямых правил доступа, выданных этой учетной записи
	/// </summary>
	public ICollection<AccessRule> AccessRules { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита, сделанных этим пользователем
	/// </summary>
	public ICollection<AuditLog> AuditActions { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита по этой учетной записи
	/// </summary>
	public ICollection<AuditLog> AuditLogs { get; set; } = [];

	/// <summary>
	/// Список актуальных сессий
	/// </summary>
	public ICollection<UserSession> Sessions { get; set; } = [];

	/// <summary>
	/// Рассчитаные для этой учетной записи указания фактического доступа
	/// </summary>
	public ICollection<CalculatedAccessRule> CalculatedAccessRules { get; set; } = [];

	#endregion Связи
}
