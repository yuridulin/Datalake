using Datalake.Domain.ValueObjects;
using System.Threading.Channels;

namespace Datalake.Data.Application.Interfaces.DataCollection;

public interface IDataCollector
{
	void Start(CancellationToken cancellationToken);

	void PrepareToStop();

	void Stop();

	string Name { get; }

	Channel<IEnumerable<TagHistory>> OutputChannel { get; }
}
