using Datalake.PublicApi.Models.Blocks;

namespace Datalake.PublicApi.Models.Tags;

/// <summary>
/// Краткая информация о блоке, имеющем связь с тегом, включая локальное имя тега в блоке
/// </summary>
public class TagBlockRelationInfo : BlockSimpleInfo
{
	/// <summary>
	/// Локальное имя тега в блоке
	/// </summary>
	public string? LocalName { get; set; }
}
