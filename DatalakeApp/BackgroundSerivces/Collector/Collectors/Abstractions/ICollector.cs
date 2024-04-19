using DatalakeApp.BackgroundSerivces.Collector.Models;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;

public delegate void CollectEvent(IEnumerable<CollectValue> values);

public interface ICollector
{
	public Task Start();

	public Task Stop();

	public event CollectEvent CollectValues;
}
