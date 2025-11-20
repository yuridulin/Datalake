using Microsoft.AspNetCore.Mvc;

namespace Datalake.Shared.Hosting.AbstractControllers.Data;

/// <summary>
/// Управление системой сбора данных
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Data")]
[Route("api/v1/data/system")]
public abstract class DataSystemControllerBase : ControllerBase
{
	/// <summary>
	/// Перезапуск системы сбора данных
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	[HttpPost]
	public abstract Task<ActionResult> RestartCollectionAsync(
		CancellationToken ct = default);
}
