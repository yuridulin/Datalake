using Microsoft.Extensions.Hosting;

namespace Datalake.Inventory.Application.Interfaces;

public interface IInfrastructureStartService
{
	Task StartAsync();
}
