using Datalake.DataService.Abstractions;
using Datalake.DataService.Database.Entities;
using Datalake.DataService.Database.Interfaces;
using Datalake.Shared.Application;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Values;

namespace Datalake.DataService.Services.Values;

[Singleton]
public class SystemWriteValuesService(
	ITagsStore tagsStore,
	ICurrentValuesStore currentValuesStore,
	IServiceScopeFactory serviceScopeFactory,
	ITagHistoryFactory tagHistoryFactory) : ISystemWriteValuesService
{
	public async Task WriteAsync(IEnumerable<ValueWriteRequest> requests)
	{
		List<TagHistory> records = [];

		foreach (var request in requests)
		{
			TagCacheInfo? tag = null;

			if (request.Id.HasValue)
				tag = tagsStore.TryGet(request.Id.Value);
			else if (request.Guid.HasValue)
				tag = tagsStore.TryGet(request.Guid.Value);

			if (tag == null)
				continue;

			var record = tagHistoryFactory.CreateFrom(tag, request);

			// проверка на уникальность (новизну)
			if (!currentValuesStore.IsNew(record.TagId, record))
				continue;

			records.Add(record);
		}

		var uniqueRecords = records
			.GroupBy(x => new { x.TagId, x.Date })
			.Select(g => g.First())
			.ToList();

		using var scope = serviceScopeFactory.CreateScope();
		var writeHistoryRepository = scope.ServiceProvider.GetRequiredService<IWriteHistoryRepository>();

		var writeResult = await writeHistoryRepository.WriteAsync(uniqueRecords);
		if (writeResult)
		{
			// обновление в кэше текущих данных
			foreach (var record in uniqueRecords)
				currentValuesStore.TryUpdate(record.TagId, record);
		}
	}
}
