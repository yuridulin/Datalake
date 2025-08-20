using Datalake.PublicApi.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с тегами
/// </summary>
[ApiController]
[Route("api/" + ControllerRoute)]
public abstract class TagsControllerBase : ControllerBase
{
	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "tags";

	/// <summary>
	/// Создание нового тега
	/// </summary>
	/// <param name="tagCreateRequest">Необходимые данные для создания тега</param>
	/// <returns>Идентификатор нового тега в локальной базе данных</returns>
	[HttpPost]
	public abstract Task<ActionResult<TagInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest tagCreateRequest);

	/// <summary>
	/// Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	/// <returns>Объект информации о теге</returns>
	[HttpGet("{id}")]
	public abstract ActionResult<TagFullInfo> Get(
			[BindRequired, FromRoute] int id);

	/// <summary>
	/// Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <param name="id">Список локальных идентификаторов тегов</param>
	/// <param name="names">Список текущих наименований тегов</param>
	/// <param name="guids">Список глобальных идентификаторов тегов</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet]
	public abstract ActionResult<TagInfo[]> GetAll(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? id,
		[FromQuery] string[]? names,
		[FromQuery] Guid[]? guids);

	/// <summary>
	/// Изменение тега
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	/// <param name="tag">Новые данные тега</param>
	[HttpPut("{id}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] TagUpdateRequest tag);

	/// <summary>
	/// Удаление тега
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	[HttpDelete("{id}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id);
}