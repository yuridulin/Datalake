using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.CreateUserGroup;

public record CreateUserGroupCommand(
	UserAccessEntity User) : ICommandRequest;
