using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeDatabase.Models;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors;

public class SystemCollector : ICollector
{
	public SystemCollector(Source source)
	{
	}

	public int[] Tags { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	public event CollectEvent CollectValues;

	public Task Start()
	{
		throw new NotImplementedException();
	}

	public Task Stop()
	{
		throw new NotImplementedException();
	}
}
