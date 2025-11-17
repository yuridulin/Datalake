namespace Datalake.Contracts.Models.Tags;

public record TagMetricRequest
{
	public IEnumerable<int>? TagsId { get; set; }

	public IEnumerable<Guid>? TagsGuid { get; set; }
}
