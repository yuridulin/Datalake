using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.DeleteSource;

public record DeleteSourceCommand(
	UserAccessEntity User,
	int SourceId) : ICommandRequest;
