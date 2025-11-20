using Datalake.Domain.Entities;

namespace Datalake.Gateway.Application.Interfaces.Storage;

public interface ISessionsStore
{
	UserSession? Get(string token);

	void Set(string token, UserSession session);

	void Remove(string token);
}
