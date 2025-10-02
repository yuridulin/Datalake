namespace Datalake.Inventory.Domain.Interfaces;

public interface ISoftDeletable
{
	bool IsDeleted { get; }

	void MarkAsDeleted();
}
