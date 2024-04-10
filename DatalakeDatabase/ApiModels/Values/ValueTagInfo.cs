using DatalakeDatabase.Enums;

namespace DatalakeDatabase.ApiModels.Values;

public class ValueTagInfo
{
	public string TagName { get; set; } = string.Empty;

	public TagType TagType { get; set; }
}
