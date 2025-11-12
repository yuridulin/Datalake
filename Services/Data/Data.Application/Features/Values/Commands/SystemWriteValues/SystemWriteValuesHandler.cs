using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Commands.SystemWriteValues;

public interface ISystemWriteValuesHandler : ICommandHandler<SystemWriteValuesCommand, bool> { }

public class SystemWriteValuesHandler(
	ITagsValuesRepository tagsHistoryRepository,
	ICurrentValuesStore currentValuesStore) : ISystemWriteValuesHandler
{
	public async Task<bool> HandleAsync(SystemWriteValuesCommand command, CancellationToken ct = default)
	{
		var uniqueRecords = command.Values
			.GroupBy(x => new { x.TagId, x.Date })
			.Select(g => g.First())
			.ToList();

		var writeResult = await tagsHistoryRepository.WriteAsync(uniqueRecords);
		if (!writeResult)
			return false;

		// обновление в кэше текущих данных
		currentValuesStore.TryUpdate(uniqueRecords);

		return true;
	}
}
