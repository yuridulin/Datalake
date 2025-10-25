using Datalake.Domain.Entities;
using System.Threading.Channels;

namespace Datalake.Data.Application.Interfaces.DataCollection;

public interface IDataCollector
{
	void Start(CancellationToken stoppingToken = default);

	void PrepareToStop();

	void Stop();

	string Name { get; }

	Channel<IEnumerable<TagValue>> OutputChannel { get; }
}
