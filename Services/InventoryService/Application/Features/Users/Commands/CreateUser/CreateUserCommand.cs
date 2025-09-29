using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
	UserAccessEntity User) : ICommandRequest;
