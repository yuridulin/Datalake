using DatalakeDatabase.Enums;

namespace DatalakeApp.BackgroundSerivces.Collector.Models;

public struct CollectValue
{
	public DateTime DateTime { get; set; }

	public int TagId { get; set; }

	public string Name { get; set; }

	public object Value { get; set; }

	public TagQuality Quality { get; set; }
}
