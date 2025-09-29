using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Sources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с источниками данных
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class SourcesControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "sources";

	/// <inheritdoc cref="CreateEmptyAsync()" />
	public const string CreateEmpty = "empty";

	/// <inheritdoc cref="CreateAsync(SourceInfo)" />
	public const string Create = "";

	/// <inheritdoc cref="GetAsync(int)" />
	public const string Get = "{id}";

	/// <inheritdoc cref="GetAllAsync(bool)" />
	public const string GetAll = "";

	/// <inheritdoc cref="UpdateAsync(int, SourceUpdateRequest)" />
	public const string Update = "{id}";

	/// <inheritdoc cref="DeleteAsync(int)" />
	public const string Delete = "{id}";

	/// <inheritdoc cref="GetItemsAsync(int)" />
	public const string GetItems = "{id}/items";

	/// <inheritdoc cref="GetItemsWithTagsAsync(int)" />
	public const string GetItemsWithTags = "{id}/items-and-tags";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание источника с информацией по умолчанию
	/// </summary>
	/// <returns>Идентификатор источника</returns>
	[HttpPost(CreateEmpty)]
	public abstract Task<ActionResult<SourceInfo>> CreateEmptyAsync();

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание источника на основе переданных данных
	/// </summary>
	/// <param name="source">Данные нового источника</param>
	/// <returns>Идентификатор источника</returns>
	[HttpPost(Create)]
	public abstract Task<ActionResult<SourceInfo>> CreateAsync(
		[BindRequired, FromBody] SourceInfo source);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение данных о источнике
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Данные о источнике</returns>
	/// <exception cref="NotFoundException">Источник не найден по идентификатору</exception>
	[HttpGet(Get)]
	public abstract Task<ActionResult<SourceInfo>> GetAsync(
		[BindRequired, FromRoute] int id);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка источников
	/// </summary>
	/// <param name="withCustom">Включить ли в список системные источники</param>
	/// <returns>Список источников</returns>
	[HttpGet(GetAll)]
	public abstract Task<ActionResult<SourceInfo[]>> GetAllAsync(bool withCustom = false);

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <param name="request">Новые данные источника</param>
	[HttpPut(Update)]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] SourceUpdateRequest request);

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	[HttpDelete(Delete)]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id);

	#endregion Методы
}
