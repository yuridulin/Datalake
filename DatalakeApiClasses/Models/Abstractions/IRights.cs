namespace DatalakeApiClasses.Models.Abstractions;

public interface IRights
{
	public Guid? UserGuid { get; set; }


	public bool? HasAccessToTag { get; set; }

	public bool? CanManageTag { get; set; }

	public bool? CanWriteToTag { get; set; }


	public bool? HasAccessToBlock { get; set; }

	public bool? CanManageBlock { get; set; }

	public bool? CanManageBlockTags { get; set; }


	public bool? HasAccessToSource { get; set; }

	public bool? CanManageSource { get; set; }

	public bool? CanManageSourceTags { get; set; }


	public bool? CanControlAccess { get; set; }

	public bool? CanViewSystemTags { get; set; }

	public bool? CanViewLogs { get; set; }
}
