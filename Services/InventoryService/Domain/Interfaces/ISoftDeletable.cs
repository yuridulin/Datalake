namespace Datalake.InventoryService.Domain.Interfaces;

public interface ISoftDeletable
{
	bool IsDeleted { get; }

	void MarkAsDeleted();
}
