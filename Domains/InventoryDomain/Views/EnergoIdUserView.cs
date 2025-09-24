using Datalake.Database.Constants;
using Datalake.Database.Tables;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Views;

/// <summary>
/// Информация о пользователе на основе EnergoId
/// </summary>
[Table(TableName, Schema = Db.EnergoIdSchema), LinqToDB.Mapping.Table(TableName, Schema = Db.EnergoIdSchema, IsView = true)]
public class EnergoIdUserView
{
	internal const string TableName = Db.EnergoIdView;

	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	[Column, PrimaryKey, Key, Required]
	public Guid Guid { get; set; }

	/// <summary>
	/// Почтовый адрес
	/// </summary>
	[Column]
	public string? Email { get; set; }

	/// <summary>
	/// Имя учетной записи
	/// </summary>
	[Column]
	public string? UserName { get; set; }

	/// <summary>
	/// Имя
	/// </summary>
	[Column]
	public string? FirstName { get; set; }

	/// <summary>
	/// Отчество
	/// </summary>
	[Column]
	public string? MiddleName { get; set; }

	/// <summary>
	/// Фамилия
	/// </summary>
	[Column]
	public string? LastName { get; set; }

	/// <summary>
	/// Используется или нет
	/// </summary>
	[Column, Required]
	public bool IsEnabled { get; set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	[Column]
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Код организации?
	/// </summary>
	[Column]
	public string? UploaderEnterpriseCode { get; set; }

	/// <summary>
	/// Код организации
	/// </summary>
	[Column]
	public string? EnterpriseCode { get; set; }

	/// <summary>
	/// Персональный номер
	/// </summary>
	[Column]
	public string? PersonnelNumber { get; set; }

	/// <summary>
	/// Номер
	/// </summary>
	[Column]
	public string? Phone { get; set; }

	/// <summary>
	/// Рабочий номер
	/// </summary>
	[Column]
	public string? WorkPhone { get; set; }

	/// <summary>
	/// Номер мобильного
	/// </summary>
	[Column]
	public string? MobilePhone { get; set; }

	/// <summary>
	/// Гендер
	/// </summary>
	[Column]
	public string? Gender { get; set; }

	/// <summary>
	/// Дата рождения
	/// </summary>
	[Column]
	public string? Birthday { get; set; }


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
	[NotMapped]
	public string FullName => $"{LastName} {FirstName} {MiddleName}";

	#endregion
}