using System.ComponentModel.DataAnnotations;

namespace Datalake.Domain.Entities;

/// <summary>
/// Информация о пользователе на основе EnergoId
/// </summary>
public class EnergoIdEntity
{
	private EnergoIdEntity() { }

	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	[Required]
	public Guid Guid { get; set; }

	/// <summary>
	/// Почтовый адрес
	/// </summary>
	public string? Email { get; set; }

	/// <summary>
	/// Имя учетной записи
	/// </summary>
	public string? UserName { get; set; }

	/// <summary>
	/// Имя
	/// </summary>
	public string? FirstName { get; set; }

	/// <summary>
	/// Отчество
	/// </summary>
	public string? MiddleName { get; set; }

	/// <summary>
	/// Фамилия
	/// </summary>
	public string? LastName { get; set; }

	/// <summary>
	/// Используется или нет
	/// </summary>
	[Required]
	public bool IsEnabled { get; set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Код организации?
	/// </summary>
	public string? UploaderEnterpriseCode { get; set; }

	/// <summary>
	/// Код организации
	/// </summary>
	public string? EnterpriseCode { get; set; }

	/// <summary>
	/// Персональный номер
	/// </summary>
	public string? PersonnelNumber { get; set; }

	/// <summary>
	/// Номер
	/// </summary>
	public string? Phone { get; set; }

	/// <summary>
	/// Рабочий номер
	/// </summary>
	public string? WorkPhone { get; set; }

	/// <summary>
	/// Номер мобильного
	/// </summary>
	public string? MobilePhone { get; set; }

	/// <summary>
	/// Гендер
	/// </summary>
	public string? Gender { get; set; }

	/// <summary>
	/// Дата рождения
	/// </summary>
	public string? Birthday { get; set; }


	#region Связи

	/// <summary>
	/// Связь с пользователем приложения
	/// </summary>
	public UserEntity? User { get; set; }

	#endregion


	#region Свойства

	/// <summary>
	/// ФИО пользователя
	/// </summary>
	public string GetFullName() => $"{LastName} {FirstName} {MiddleName}";

	#endregion
}