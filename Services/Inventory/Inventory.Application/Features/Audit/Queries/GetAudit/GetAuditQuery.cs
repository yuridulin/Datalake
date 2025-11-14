using Datalake.Contracts.Models.LogModels;
using Datalake.Domain.Enums;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Audit.Queries.GetAudit;

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
	Guid? AuthorGuid = null) : IQueryRequest<IEnumerable<LogInfo>>;
