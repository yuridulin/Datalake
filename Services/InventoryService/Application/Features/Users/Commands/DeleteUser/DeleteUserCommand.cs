using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(
	UserAccessEntity User) : ICommandRequest;
