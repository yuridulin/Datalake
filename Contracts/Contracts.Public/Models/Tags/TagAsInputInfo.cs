using Datalake.Contracts.Public.Interfaces;
using Datalake.Contracts.Public.Models;

namespace Datalake.Contracts.Public.Models.Tags;

/// <summary>
/// Информации о теге, выступающем в качестве входящей переменной при составлении формулы
/// </summary>
public class TagAsInputInfo : TagSimpleInfo, IProtectedEntity
{
	/// <summary>
	/// Идентификатор блока, в котором тег был выбран. Если не укзан, то тег выбран из не распределенных
	/// </summary>
	public int? BlockId { get; set; }

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
