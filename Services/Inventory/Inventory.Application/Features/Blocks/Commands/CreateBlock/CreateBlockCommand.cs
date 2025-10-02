using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.CreateBlock;

public record CreateBlockCommand(
	UserAccessEntity User,
	int? ParentId,
	string? Name,
	string? Description) : ICommandRequest;
