using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Interfaces;

public interface IWithUserAccess
{
	UserAccessEntity User { get; init; }
}
