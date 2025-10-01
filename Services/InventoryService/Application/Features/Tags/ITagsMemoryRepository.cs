using Datalake.InventoryService.Infrastructure.Database;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.InventoryService.Application.Features.Tags;

public interface ITagsMemoryRepository
{
	Task<TagInfo> CreateAsync(InventoryEfContext db, UserAuthInfo user, TagCreateRequest tagCreateRequest);
	Task DeleteAsync(InventoryEfContext db, UserAuthInfo user, int id);
	TagFullInfo Get(UserAuthInfo user, int id);
	TagInfo[] GetAll(UserAuthInfo user, int? sourceId, int[]? id, string[]? names, Guid[]? guids);
	Task UpdateAsync(InventoryEfContext db, UserAuthInfo user, int id, TagUpdateRequest updateRequest);
}