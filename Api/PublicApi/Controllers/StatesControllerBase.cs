using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.States;
using Datalake.PublicApi.Models.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Представление системной информации о работе сервера
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class StatesControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "states";

	/// <inheritdoc cref="GetUsersAsync" />
	public const string Users = "users";

	/// <inheritdoc cref="GetSourcesAsync" />
	public const string Sources = "sources";

	/// <inheritdoc cref="GetTagsAsync" />
	public const string Tags = "tags";

	/// <inheritdoc cref="GetTagAsync" />
	public const string Tag = "tags/{id}";

	/// <inheritdoc cref="GetValuesAsync" />
	public const string Values = "values";

	/// <inheritdoc cref="GetTagsReceiveAsync" />
	public const string TagsReceive = "calculation";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Информация о визитах пользователей
	/// </summary>
	/// <returns>Даты визитов, сопоставленные с идентификаторами пользователей</returns>
	[HttpGet(Users)]
	public abstract Task<ActionResult<Dictionary<Guid, DateTime>>> GetUsersAsync();

	#endregion Методы
}
