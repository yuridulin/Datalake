using Datalake.Shared.Infrastructure.Database.Repositories;

namespace Datalake.Gateway.Infrastructure.Database.Repositories;

public class UserAccessValuesRepository(GatewayDbContext context) : AbstractUserAccessValuesRepository(context)
{
}
