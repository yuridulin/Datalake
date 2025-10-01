using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }

	public string? Login { get; init; }

	public string? FullName { get; init; }

	public required AccessType AccessType { get; init; }

	public required UserType Type { get; init; }

	public string? Password { get; init; }

	public string? StaticHost { get; init; }

	public Guid? EnergoIdGuid { get; init; }

	public bool GenerateNewHash { get; init; }
}
