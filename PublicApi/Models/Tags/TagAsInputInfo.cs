using Datalake.PublicApi.Abstractions;
using Datalake.PublicApi.Models.Auth;

namespace Datalake.PublicApi.Models.Tags;

/// <summary>
/// Информации о теге, выступающем в качестве входящей переменной при составлении формулы
/// </summary>
public class TagAsInputInfo : TagSimpleInfo, IProtectedEntity
{
	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
