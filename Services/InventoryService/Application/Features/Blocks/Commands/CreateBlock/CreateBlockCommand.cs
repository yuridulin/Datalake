using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.CreateBlock;

public record CreateBlockCommand(
	UserAccessEntity User,
	int? ParentId,
	string? Name,
	string? Description) : ICommandRequest;
