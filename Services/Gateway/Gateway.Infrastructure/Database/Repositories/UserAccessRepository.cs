using Datalake.Shared.Infrastructure.Repositories;

namespace Datalake.Gateway.Infrastructure.Database.Repositories;

public class UserAccessRepository(GatewayDbContext context) : UserAccessAbstractRepository(context)
{
}
