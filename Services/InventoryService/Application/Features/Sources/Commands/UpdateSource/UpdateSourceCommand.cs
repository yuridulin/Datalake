using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.UpdateSource;

public record UpdateSourceCommand(
	UserAccessEntity User) : ICommandRequest;
