using DatalakeApiClasses.Models.Abstractions;

namespace DatalakeApiClasses.Models.Users;

public class UserRights : IRights
{
	public Guid? UserGuid { get; set; } = null;


	public bool? HasAccessToTag { get; set; } = false;

	public bool? CanManageTag { get; set; } = false;

	public bool? CanWriteToTag { get; set; } = false;


	public bool? HasAccessToBlock { get; set; } = false;

	public bool? CanManageBlock { get; set; } = false;

	public bool? CanManageBlockTags { get; set; } = false;


	public bool? HasAccessToSource { get; set; } = false;

	public bool? CanManageSource { get; set; } = false;

	public bool? CanManageSourceTags { get; set; } = false;


	public bool? CanControlAccess { get; set; } = false;

	public bool? CanViewSystemTags { get; set; } = false;

	public bool? CanViewLogs { get; set; } = false;
}
