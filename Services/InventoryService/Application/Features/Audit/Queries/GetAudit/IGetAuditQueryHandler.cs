using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Models.LogModels;

namespace Datalake.InventoryService.Application.Features.Audit.Queries.Audit;

public interface IGetAuditQueryHandler : IQueryHandler<GetAuditQuery, IEnumerable<LogInfo>>
{
}
