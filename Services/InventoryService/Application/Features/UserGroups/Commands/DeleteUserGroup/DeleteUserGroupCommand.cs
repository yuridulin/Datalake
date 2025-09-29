using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.DeleteUserGroup;

public record DeleteUserGroupCommand(
	UserAccessEntity User) : ICommandRequest;
