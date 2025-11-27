using Datalake.Contracts.Models.Data.Values;
using Datalake.Contracts.Models.Sources;
using Datalake.Data.Application.Interfaces;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Sources.Queries.GetRemoteItems;

public interface IGetSourceRemoteItemsHandler : IQueryHandler<GetSourceRemoteItemsQuery, List<SourceItemInfo>> { }

public class GetSourceRemoteItemsHandler(
	ISourcesRepository sourcesRepository,
	IReceiverService receiverService) : IGetSourceRemoteItemsHandler
{
	public async Task<List<SourceItemInfo>> HandleAsync(
		GetSourceRemoteItemsQuery query,
		CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToSource(Domain.Enums.AccessType.Editor, query.SourceId);

		var source = await sourcesRepository.GetByIdAsync(query.SourceId, ct)
			?? throw new ApplicationException("Источник данных не найден");

		var sourceItemsResponse = await receiverService.AskSourceAsync(
			sourceType: source.Type,
			address: source.Address);

		var date = DateTime.UtcNow;
		var sourceItems = sourceItemsResponse.Tags
			.DistinctBy(x => x.Name)
			.Select(x =>
			{
				var value = TagValue.FromRaw(0, x.Type, date, x.Quality, x.Value, null);
				return new SourceItemInfo
				{
					Path = x.Name,
					Type = x.Type,
					Value = new ValueRecord
					{
						Date = value.Date,
						Boolean = value.Boolean,
						Number = value.Number,
						Text = value.Text,
						Quality = value.Quality,
					}
				};
			})
			.ToList();

		return sourceItems;
	}
}
