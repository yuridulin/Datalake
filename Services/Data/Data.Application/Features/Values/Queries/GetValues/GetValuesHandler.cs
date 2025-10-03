using Datalake.Data.Api.Models.Values;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Queries.GetValues;

public interface IGetValuesHandler : IQueryHandler<GetValuesQuery, IEnumerable<ValuesResponse>> { }

public class GetValuesHandler : IGetValuesHandler
{
	public Task<IEnumerable<ValuesResponse>> HandleAsync(GetValuesQuery query, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
