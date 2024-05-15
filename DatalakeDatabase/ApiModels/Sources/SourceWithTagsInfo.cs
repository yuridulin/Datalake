using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Sources;

public class SourceWithTagsInfo: SourceInfo
{
	[Required]
	public IEnumerable<SourceTagInfo> Tags { get; set; } = [];
}
