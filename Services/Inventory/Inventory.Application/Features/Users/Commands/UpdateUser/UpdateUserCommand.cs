using Datalake.Domain.Enums;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required Guid Guid { get; init; }

	public string? Login { get; init; }

	public string? Password { get; init; }

	public string? FullName { get; init; }

	public string? Email { get; init; }

	public required AccessType AccessType { get; init; }
}
