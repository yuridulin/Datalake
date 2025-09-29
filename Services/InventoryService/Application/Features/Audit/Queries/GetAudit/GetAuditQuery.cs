using Datalake.InventoryService.Application.Interfaces;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.LogModels;

namespace Datalake.InventoryService.Application.Features.Audit.Queries.GetAudit;

public record GetAuditQuery(
	int? LastId = null,
	int? FirstId = null,
	int? Take = null,
	int? SourceId = null,
	int? BlockId = null,
	Guid? TagGuid = null,
	Guid? UserGuid = null,
	Guid? GroupGuid = null,
	LogCategory[]? Categories = null,
	LogType[]? Types = null,
	Guid? AuthorGuid = null) : IQuery<IEnumerable<LogInfo>>;
