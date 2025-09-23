using Datalake.Database;
using Datalake.Database.Constants;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Sources;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Maintenance;
using Datalake.Server.Services.Receiver;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class SourcesController(
	DatalakeContext db,
	AuthenticationService authenticator,
	SourcesMemoryRepository sourcesRepository,
	ReceiverService receiverService,
	TagsStateService tagsStateService) : SourcesControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<SourceInfo>> CreateEmptyAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		var info = await sourcesRepository.CreateAsync(db, user);

		return info;
	}

	/// <inheritdoc />
	public override async Task<ActionResult<SourceInfo>> CreateAsync(
		[BindRequired, FromBody] SourceInfo source)
	{
		var user = authenticator.Authenticate(HttpContext);

		var info = await sourcesRepository.CreateAsync(db, user, source);

		return info;
	}

	/// <inheritdoc />
	public override async Task<ActionResult<SourceInfo>> GetAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(sourcesRepository.Get(user, id));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<SourceInfo[]>> GetAllAsync(bool withCustom = false)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(sourcesRepository.GetAll(user, withCustom));
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] SourceUpdateRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		await sourcesRepository.UpdateAsync(db, user, id, request);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		await sourcesRepository.DeleteAsync(db, user, id);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult<SourceItemInfo[]>> GetItemsAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoAccessToSource(user, PublicApi.Enums.AccessType.Viewer, id);

		var source = sourcesRepository.Get(user, id);
		var sourceItemsResponse = await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);

		var items = sourceItemsResponse.Tags
			.Select(x => new SourceItemInfo
			{
				Type = x.Type,
				Path = x.Name,
				Value = x.Value,
			})
			.OrderBy(x => x.Path)
			.ToArray();

		return items;
	}

	/// <inheritdoc />
	public override async Task<ActionResult<SourceEntryInfo[]>> GetItemsWithTagsAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		AccessChecks.ThrowIfNoAccessToSource(user, PublicApi.Enums.AccessType.Editor, id);

		var source = sourcesRepository.GetWithTags(user, id);

		var sourceItemsResponse = await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);
		var sourceItems = sourceItemsResponse.Tags
			.DistinctBy(x => x.Name)
			.ToDictionary(x => x.Name, x => new SourceItemInfo { Path = x.Name, Type = x.Type, Value = x.Value, Quality = x.Quality });

		var sourceTags = source.Tags.ToList();
		var tagsStates = tagsStateService.GetTagsStates();

		var all = sourceTags
			.Select(tag => new SourceEntryInfo
			{
				TagInfo = tag,
				ItemInfo = sourceItems.TryGetValue(tag.Item, out var itemInfo) ? itemInfo : null,
				IsTagInUse = tagsStates.TryGetValue(tag.Id, out var metrics) ? metrics.Where(x => !Lists.InnerRequests.Contains(x.Key)).Max(x => x.Value) : null,
			})
			.Union(sourceItems
				.Where(itemKeyValue => !sourceTags.Select(tag => tag.Item).Contains(itemKeyValue.Key))
				.Select(itemKeyValue => new SourceEntryInfo
				{
					TagInfo = null,
					ItemInfo = itemKeyValue.Value,
					IsTagInUse = null,
				}));

		return all
			.OrderBy(x => x.ItemInfo?.Path)
			.ThenBy(x => x.TagInfo?.Item)
			.ToArray();
	}
}