using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.CreateSource;

public record CreateSourceCommand(
	UserAccessEntity User,
	string? Name,
	string? Description,
	string? Address,
	SourceType Type = SourceType.Inopc) : ICommandRequest;
