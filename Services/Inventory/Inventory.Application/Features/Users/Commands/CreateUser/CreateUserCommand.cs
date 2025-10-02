using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;

namespace Datalake.Inventory.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public string? Login { get; init; }

	public string? FullName { get; init; }

	public required AccessType AccessType { get; init; }

	public required UserType Type { get; init; }

	public string? Password { get; init; }

	public string? StaticHost { get; init; }

	public Guid? EnergoIdGuid { get; init; }
}
