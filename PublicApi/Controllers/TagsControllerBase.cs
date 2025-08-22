using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с тегами
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class TagsControllerBase : ControllerBase
{
	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "tags";

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание нового тега
	/// </summary>
	/// <param name="tagCreateRequest">Необходимые данные для создания тега</param>
	/// <returns>Идентификатор нового тега в локальной базе данных</returns>
	[HttpPost]
	public abstract Task<ActionResult<TagInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest tagCreateRequest);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	/// <returns>Объект информации о теге</returns>
	[HttpGet("{id}")]
	public abstract Task<ActionResult<TagFullInfo>> GetAsync(
			[BindRequired, FromRoute] int id);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <param name="id">Список локальных идентификаторов тегов</param>
	/// <param name="names">Список текущих наименований тегов</param>
	/// <param name="guids">Список глобальных идентификаторов тегов</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet]
	public abstract Task<ActionResult<TagInfo[]>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? id,
		[FromQuery] string[]? names,
		[FromQuery] Guid[]? guids);

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение тега
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	/// <param name="tag">Новые данные тега</param>
	[HttpPut("{id}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] TagUpdateRequest tag);

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление тега
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	[HttpDelete("{id}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id);
}