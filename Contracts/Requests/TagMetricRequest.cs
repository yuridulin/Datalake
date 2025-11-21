namespace Datalake.Contracts.Requests;

public record TagMetricRequest
{
	public IEnumerable<int>? TagsId { get; set; }

	public IEnumerable<Guid>? TagsGuid { get; set; }
}
