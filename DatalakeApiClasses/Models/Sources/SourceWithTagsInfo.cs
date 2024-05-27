using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Sources;

public class SourceWithTagsInfo: SourceInfo
{
	[Required]
	public IEnumerable<SourceTagInfo> Tags { get; set; } = [];
}
