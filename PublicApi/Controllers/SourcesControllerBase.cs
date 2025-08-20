using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Sources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с источниками данных
/// </summary>
[Route("api/" + ControllerRoute)]
[ApiController]
public abstract class SourcesControllerBase : ControllerBase
{
	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "sources";

	/// <summary>
	/// Создание источника с информацией по умолчанию
	/// </summary>
	/// <returns>Идентификатор источника</returns>
	[HttpPost("empty")]
	public abstract Task<ActionResult<SourceInfo>> CreateEmptyAsync();

	/// <summary>
	/// Создание источника на основе переданных данных
	/// </summary>
	/// <param name="source">Данные нового источника</param>
	/// <returns>Идентификатор источника</returns>
	[HttpPost]
	public abstract Task<ActionResult<SourceInfo>> CreateAsync(
		[BindRequired, FromBody] SourceInfo source);

	/// <summary>
	/// Получение данных о источнике
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Данные о источнике</returns>
	/// <exception cref="NotFoundException">Источник не найден по идентификатору</exception>
	[HttpGet("{id:int}")]
	public abstract Task<ActionResult<SourceInfo>> GetAsync(
		[BindRequired, FromRoute] int id);

	/// <summary>
	/// Получение списка источников
	/// </summary>
	/// <param name="withCustom">Включить ли в список системные источники</param>
	/// <returns>Список источников</returns>
	[HttpGet]
	public abstract Task<ActionResult<SourceInfo[]>> GetAllAsync(bool withCustom = false);

	/// <summary>
	/// Изменение источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <param name="request">Новые данные источника</param>
	[HttpPut("{id:int}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] SourceUpdateRequest request);

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	[HttpDelete("{id:int}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id);

	/// <summary>
	/// Получение доступных значений источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Список данных источника</returns>
	/// <exception cref="NotFoundException"></exception>
	[HttpGet("{id:int}/items")]
	public abstract Task<ActionResult<SourceItemInfo[]>> GetItemsAsync(
		[BindRequired, FromRoute] int id);

	/// <summary>
	/// Получение доступных значений и связанных тегов источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Список данных источника</returns>
	/// <exception cref="NotFoundException"></exception>
	[HttpGet("{id:int}/items-and-tags")]
	public abstract Task<ActionResult<SourceEntryInfo[]>> GetItemsWithTagsAsync(
		[BindRequired, FromRoute] int id);
}