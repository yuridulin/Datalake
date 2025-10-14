using Datalake.Domain.ValueObjects;

namespace Datalake.Shared.Application.Interfaces;

public interface IWithUserAccess
{
	UserAccessValue User { get; init; }
}
