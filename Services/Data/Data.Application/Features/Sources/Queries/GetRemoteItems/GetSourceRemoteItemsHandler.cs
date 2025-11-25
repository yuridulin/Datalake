using Datalake.Contracts.Models.Sources;
using Datalake.Data.Application.Interfaces;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Sources.Queries.GetRemoteItems;

public interface IGetSourceRemoteItemsHandler : IQueryHandler<GetSourceRemoteItemsQuery, IEnumerable<SourceItemInfo>> { }

public class GetSourceRemoteItemsHandler(
	ISourcesRepository sourcesRepository,
	IReceiverService receiverService) : IGetSourceRemoteItemsHandler
{
	public async Task<IEnumerable<SourceItemInfo>> HandleAsync(
		GetSourceRemoteItemsQuery query,
		CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToSource(Domain.Enums.AccessType.Editor, query.SourceId);

		var source = await sourcesRepository.GetByIdAsync(query.SourceId, ct)
			?? throw new ApplicationException("Источник данных не найден");

		var sourceItemsResponse = await receiverService.AskSourceAsync(
			sourceType: source.Type,
			address: source.Address);

		var sourceItems = sourceItemsResponse.Tags
			.DistinctBy(x => x.Name)
			.Select(x => new SourceItemInfo { Path = x.Name, Type = x.Type, Value = x.Value, Quality = x.Quality })
			.ToArray();

		return sourceItems;
	}
}
