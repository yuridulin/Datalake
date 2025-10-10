using System.ComponentModel.DataAnnotations;

namespace Datalake.Domain.Entities;

/// <summary>
/// Информация о пользователе на основе EnergoId
/// </summary>
public class EnergoId
{
	private EnergoId() { }

	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid Guid { get; private set; }

	/// <summary>
	/// Почтовый адрес
	/// </summary>
	public string? Email { get; private set; }

	/// <summary>
	/// Имя учетной записи
	/// </summary>
	public string? UserName { get; private set; }

	/// <summary>
	/// Имя
	/// </summary>
	public string? FirstName { get; private set; }

	/// <summary>
	/// Отчество
	/// </summary>
	public string? MiddleName { get; private set; }

	/// <summary>
	/// Фамилия
	/// </summary>
	public string? LastName { get; private set; }

	/// <summary>
	/// Используется или нет
	/// </summary>
	[Required]
	public bool IsEnabled { get; private set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime? CreatedAt { get; private set; }

	/// <summary>
	/// Код организации?
	/// </summary>
	public string? UploaderEnterpriseCode { get; private set; }

	/// <summary>
	/// Код организации
	/// </summary>
	public string? EnterpriseCode { get; private set; }

	/// <summary>
	/// Персональный номер
	/// </summary>
	public string? PersonnelNumber { get; private set; }

	/// <summary>
	/// Номер
	/// </summary>
	public string? Phone { get; private set; }

	/// <summary>
	/// Рабочий номер
	/// </summary>
	public string? WorkPhone { get; private set; }

	/// <summary>
	/// Номер мобильного
	/// </summary>
	public string? MobilePhone { get; private set; }

	/// <summary>
	/// Гендер
	/// </summary>
	public string? Gender { get; private set; }

	/// <summary>
	/// Дата рождения
	/// </summary>
	public string? Birthday { get; private set; }


	#region Связи

	/// <summary>
	/// Связь с пользователем приложения
	/// </summary>
	public User? User { get; set; }

	#endregion


	#region Свойства

	/// <summary>
	/// ФИО пользователя
	/// </summary>
	public string GetFullName() => $"{LastName} {FirstName} {MiddleName}";

	#endregion
}