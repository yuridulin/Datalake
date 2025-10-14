using Datalake.Shared.Application.Entities;

namespace Datalake.Shared.Application.Interfaces;

public interface IWithUserAccess
{
	UserAccessValue User { get; init; }
}
