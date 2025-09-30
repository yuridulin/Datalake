namespace Datalake.InventoryService.Domain.Interfaces;

public interface IWithIdentityKey
{
	int Id { get; }
}

public interface IWithGuidKey
{
	Guid Guid { get; }
}

public interface ISoftDeletable
{
	bool IsDeleted { get; }

	void MarkAsDeleted();
}
