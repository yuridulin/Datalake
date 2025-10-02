using Datalake.Shared.Application.Exceptions;

namespace Datalake.Inventory.Application.Exceptions;

/// <summary>
/// Ошибки
/// </summary>
public static class InventoryNotFoundException
{
	public static NotFoundException NotFoundBlock(int blockId)
	{
		return new("BLOCK_NOT_FOUND", $"Блок с идентификатором {blockId} не найден");
	}

	public static NotFoundException NotFoundBlock(string message)
	{
		return new("BLOCK_NOT_FOUND", message);
	}

	public static NotFoundException NotFoundSource(int sourceId) => new("SOURCE_NOT_FOUND", $"Источник с идентификатором {sourceId} не найден");

	public static NotFoundException NotFoundTag(int tagId, string? details = null)
		=> new("TAG_NOT_FOUND", $"Тег с идентификатором {tagId} не найден" + (string.IsNullOrEmpty(details) ? string.Empty : $". {details}"));

	public static NotFoundException NotFoundUser(Guid userGuid) => new("USER_NOT_FOUND", $"Учетная запись с идентификатором {userGuid} не найдена");

	public static NotFoundException NotFoundUserGroup(Guid userGroupGuid) => new("USER_GROUP_NOT_FOUND", $"Группа учетных записей с идентификатором {userGroupGuid} не найдена");
}
