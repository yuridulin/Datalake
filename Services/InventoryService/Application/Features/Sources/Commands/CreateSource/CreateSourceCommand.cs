using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.CreateSource;

public record CreateSourceCommand(
	UserAccessEntity User) : ICommandRequest;
