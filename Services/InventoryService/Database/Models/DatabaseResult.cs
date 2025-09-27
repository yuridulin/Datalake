namespace Datalake.InventoryService.Database.Models;

public class DatabaseResult<TEntity>
{
	public IReadOnlyCollection<int> DeletedIdentifiers { get; set; } = [];

	public IReadOnlyCollection<TEntity> AddedEntities { get; set; } = [];

	public IReadOnlyCollection<TEntity> UpdatedEntities { get; set; } = [];

	public bool HasChanges => DeletedIdentifiers.Count > 0 || AddedEntities.Count > 0 || UpdatedEntities.Count > 0;
}
