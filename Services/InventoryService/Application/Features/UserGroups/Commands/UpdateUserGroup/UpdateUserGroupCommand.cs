using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.UpdateUserGroup;

public record UpdateUserGroupCommand(
	UserAccessEntity User) : ICommandRequest;
