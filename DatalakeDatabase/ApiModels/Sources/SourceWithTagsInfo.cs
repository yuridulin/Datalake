namespace DatalakeDatabase.ApiModels.Sources;

public class SourceWithTagsInfo: SourceInfo
{
	public IEnumerable<SourceTagInfo> Tags { get; set; } = [];
}
