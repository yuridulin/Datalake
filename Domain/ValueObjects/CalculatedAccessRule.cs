using Datalake.Domain.Entities;
using Datalake.Domain.Enums;

namespace Datalake.Domain.ValueObjects;

/// <summary>
/// Запись вычисленного доступа учетной записи глобально или на определенный объект
/// </summary>
public sealed record CalculatedAccessRule
{
	#region Создание

	/// <summary>
	/// Служебный конструктор
	/// </summary>
	private CalculatedAccessRule() { }

	/// <summary>
	/// Создание записи вычисленного глобального доступа учетной записи
	/// </summary>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="type">Уровень доступа</param>
	/// <param name="ruleId">Идентификатор правила, которое оказалось решающим при расчете</param>
	/// <returns>Запись вычисленного доступа</returns>
	public static CalculatedAccessRule Global(Guid userGuid, AccessType type, int ruleId)
	{
		return new()
		{
			UserGuid = userGuid,
			AccessType = type,
			RuleId = ruleId,
			IsGlobal = true,
			BlockId = null,
			SourceId = null,
			TagId = null,
			UserGroupGuid = null,
			UpdatedAt = DateTime.UtcNow,
		};
	}

	/// <summary>
	/// Создание записи вычисленного доступа учетной записи на конкретный тег
	/// </summary>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="type">Уровень доступа</param>
	/// <param name="ruleId">Идентификатор правила, которое оказалось решающим при расчете</param>
	/// <returns>Запись вычисленного доступа</returns>
	public static CalculatedAccessRule ForTag(Guid userGuid, int tagId, AccessType type, int ruleId)
	{
		return new()
		{
			UserGuid = userGuid,
			AccessType = type,
			RuleId = ruleId,
			IsGlobal = false,
			TagId = tagId,
			BlockId = null,
			SourceId = null,
			UserGroupGuid = null,
			UpdatedAt = DateTime.UtcNow,
		};
	}

	/// <summary>
	/// Создание записи вычисленного доступа учетной записи на конкретный блок
	/// </summary>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="type">Уровень доступа</param>
	/// <param name="ruleId">Идентификатор правила, которое оказалось решающим при расчете</param>
	/// <returns>Запись вычисленного доступа</returns>
	public static CalculatedAccessRule ForBlock(Guid userGuid, int blockId, AccessType type, int ruleId)
	{
		return new()
		{
			UserGuid = userGuid,
			AccessType = type,
			RuleId = ruleId,
			IsGlobal = false,
			TagId = null,
			BlockId = blockId,
			SourceId = null,
			UserGroupGuid = null,
			UpdatedAt = DateTime.UtcNow,
		};
	}

	/// <summary>
	/// Создание записи вычисленного доступа учетной записи на конкретный источник данных
	/// </summary>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <param name="type">Уровень доступа</param>
	/// <param name="ruleId">Идентификатор правила, которое оказалось решающим при расчете</param>
	/// <returns>Запись вычисленного доступа</returns>
	public static CalculatedAccessRule ForSource(Guid userGuid, int sourceId, AccessType type, int ruleId)
	{
		return new()
		{
			UserGuid = userGuid,
			AccessType = type,
			RuleId = ruleId,
			IsGlobal = false,
			TagId = null,
			BlockId = null,
			SourceId = sourceId,
			UserGroupGuid = null,
			UpdatedAt = DateTime.UtcNow,
		};
	}

	/// <summary>
	/// Создание записи вычисленного доступа учетной записи на конкретную группу учетных записей
	/// </summary>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="userGroupGuid">Идентификатор группы учетных записей</param>
	/// <param name="type">Уровень доступа</param>
	/// <param name="ruleId">Идентификатор правила, которое оказалось решающим при расчете</param>
	/// <returns>Запись вычисленного доступа</returns>
	public static CalculatedAccessRule ForUserGroup(Guid userGuid, Guid userGroupGuid, AccessType type, int ruleId)
	{
		return new()
		{
			UserGuid = userGuid,
			AccessType = type,
			RuleId = ruleId,
			IsGlobal = false,
			TagId = null,
			BlockId = null,
			SourceId = null,
			UserGroupGuid = userGroupGuid,
			UpdatedAt = DateTime.UtcNow,
		};
	}

	#endregion Создание

	#region Свойства

	/// <summary>
	/// Идентификатор записи
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор учетной записи
	/// </summary>
	public required Guid UserGuid { get; init; }

	/// <summary>
	/// Уровень доступа
	/// </summary>
	public required AccessType AccessType { get; init; }

	/// <summary>
	/// Определяет ли запись глобальный уровень доступа
	/// </summary>
	public required bool IsGlobal { get; init; }

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	public required int? TagId { get; init; }

	/// <summary>
	/// Идентификатор блока
	/// </summary>
	public required int? BlockId { get; init; }

	/// <summary>
	/// Идентификатор источника данных
	/// </summary>
	public required int? SourceId { get; init; }

	/// <summary>
	/// Идентификатор группы учетных записей
	/// </summary>
	public required Guid? UserGroupGuid { get; init; }

	/// <summary>
	/// Идентификатор правила
	/// </summary>
	public required int RuleId { get; init; }

	/// <summary>
	/// Время последнего изменения
	/// </summary>
	public DateTime UpdatedAt { get; init; }

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Учетная запись, для которой определен доступ
	/// </summary>
	public User User { get; private set; } = null!;

	/// <summary>
	/// Тег, доступ к которому рассчитан
	/// </summary>
	public Tag? Tag { get; private set; }

	/// <summary>
	/// Блок, доступ к которому рассчитан
	/// </summary>
	public Block? Block { get; private set; }

	/// <summary>
	/// Источник, доступ к которому рассчитан
	/// </summary>
	public Source? Source { get; private set; }

	/// <summary>
	/// Группа учетных записей, доступ к которой рассчитан
	/// </summary>
	public UserGroup? UserGroup { get; private set; }

	#endregion Связи
}
