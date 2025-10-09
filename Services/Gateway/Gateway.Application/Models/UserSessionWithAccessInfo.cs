using System.ComponentModel.DataAnnotations;

namespace Datalake.Gateway.Application.Models;

public record UserSessionWithAccessInfo : SessionInfo
{
	/// <summary>
	/// Информация о правах пользователя
	/// </summary>
	[Required]
	public required AccessInfo Access { get; init; }
}
