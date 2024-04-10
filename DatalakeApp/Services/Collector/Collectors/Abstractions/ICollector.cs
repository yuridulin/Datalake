using DatalakeApp.Services.Collector.Models;

namespace DatalakeApp.Services.Collector.Collectors.Abstractions;

public delegate void CollectEvent(IEnumerable<CollectValue> values);

public interface ICollector
{
	public Task Start();

	public Task Stop();

	public event CollectEvent CollectValues;
}
