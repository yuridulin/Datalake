namespace Datalake.Domain.Interfaces;

public interface ISoftDeletable
{
	bool IsDeleted { get; }

	void MarkAsDeleted();
}
