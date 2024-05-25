using DatalakeApp.BackgroundServices.Collector.Models;
using DatalakeDatabase.Enums;

namespace DatalakeApp.BackgroundServices.Collector.Collectors.Abstractions;

public delegate void CollectEvent(ICollector collector, IEnumerable<CollectValue> values);

public interface ICollector
{
	public string Name { get; set; }

	public SourceType Type { get; set; }

	public Task Start();

	public Task Stop();

	public event CollectEvent CollectValues;
}
