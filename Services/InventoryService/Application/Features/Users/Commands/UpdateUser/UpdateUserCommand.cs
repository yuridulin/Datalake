using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
	UserAccessEntity User) : ICommandRequest;
