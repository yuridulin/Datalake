using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Inventory.Api.Models.Blocks;
using Datalake.Inventory.Api.Models.LogModels;
using Datalake.Inventory.Api.Models.Sources;
using Datalake.Inventory.Api.Models.Tags;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;
using Datalake.Inventory.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class AuditQueriesService(InventoryEfContext context) : IAuditQueriesService
{
	public async Task<IEnumerable<LogInfo>> GetAsync(
		int? lastId = null,
		int? firstId = null,
		int? take = null,
		int? sourceId = null,
		int? blockId = null,
		Guid? tagGuid = null,
		Guid? userGuid = null,
		Guid? groupGuid = null,
		LogCategory[]? categories = null,
		LogType[]? types = null,
		Guid? authorGuid = null,
		CancellationToken ct = default)
	{
		var query = context.Audit
			.Include(x => x.Author)
			.Include(log => log.AffectedSource)
			.Include(log => log.AffectedBlock)
			.Include(log => log.AffectedTag)
					.ThenInclude(tag => tag!.Source)
			.Include(log => log.AffectedUser)
			.Include(log => log.AffectedUserGroup)
			.AsNoTracking()
			.AsQueryable();

		// Применяем фильтры для включения удаленных объектов
		query = query.Where(log =>
			(log.Author == null || !log.Author.IsDeleted) &&
			(log.AffectedSource == null || !log.AffectedSource.IsDeleted) &&
			(log.AffectedBlock == null || !log.AffectedBlock.IsDeleted) &&
			(log.AffectedTag == null || !log.AffectedTag.IsDeleted) &&
			(log.AffectedUser == null || !log.AffectedUser.IsDeleted) &&
			(log.AffectedUserGroup == null || !log.AffectedUserGroup.IsDeleted) &&
			(log.AffectedTag == null || log.AffectedTag.Source == null || !log.AffectedTag.Source.IsDeleted));

		// Применяем дополнительные фильтры
		if (authorGuid != null)
			query = query.Where(x => x.Author != null && x.Author.Guid == authorGuid.Value);

		if (sourceId != null)
			query = query.Where(x => x.AffectedSource != null && x.AffectedSource.Id == sourceId.Value);

		if (blockId != null)
			query = query.Where(x => x.AffectedBlock != null && x.AffectedBlock.Id == blockId.Value);

		if (tagGuid != null)
			query = query.Where(x => x.AffectedTag != null && x.AffectedTag.Guid == tagGuid.Value);

		if (userGuid != null)
			query = query.Where(x => x.AffectedUser != null && x.AffectedUser.Guid == userGuid.Value);

		if (groupGuid != null)
			query = query.Where(x => x.AffectedUserGroup != null && x.AffectedUserGroup.Guid == groupGuid.Value);

		if (categories != null && categories.Length > 0)
			query = query.Where(x => categories.Contains(x.Category));

		if (types != null && types.Length > 0)
			query = query.Where(x => types.Contains(x.Type));

		if (authorGuid != null)
			query = query.Where(x => x.Author != null && x.Author.Guid == authorGuid.Value);

		query = query
			.OrderByDescending(x => x.Id);

		if (lastId.HasValue)
			query = query.Where(x => x.Id > lastId.Value);
		else if (firstId.HasValue)
			query = query.Where(x => x.Id < firstId.Value);

		if (take.HasValue)
			query = query.Take(take.Value);

		// Проекция в LogInfo
		return await query
			.Select(log => new LogInfo
			{
				Id = log.Id,
				Category = log.Category,
				DateString = log.Date.HierarchicalWithMilliseconds(),
				Text = log.Text,
				Type = log.Type,
				Details = log.Details,
				Author = log.Author == null ? null : new UserSimpleInfo
				{
					Guid = log.Author.Guid,
					FullName = log.Author.FullName ?? log.Author.Login ?? string.Empty,
				},
				AffectedSource = log.AffectedSource == null ? null : new SourceSimpleInfo
				{
					Id = log.AffectedSource.Id,
					Name = log.AffectedSource.Name,
				},
				AffectedBlock = log.AffectedBlock == null ? null : new BlockSimpleInfo
				{
					Id = log.AffectedBlock.Id,
					Guid = log.AffectedBlock.GlobalId,
					Name = log.AffectedBlock.Name,
				},
				AffectedTag = log.AffectedTag == null ? null : new TagSimpleInfo
				{
					Id = log.AffectedTag.Id,
					Guid = log.AffectedTag.Guid,
					Name = log.AffectedTag.Name,
					Type = log.AffectedTag.Type,
					Resolution = log.AffectedTag.Resolution,
					SourceType = log.AffectedTag.Source == null ? SourceType.NotSet : log.AffectedTag.Source.Type,
				},
				AffectedUser = log.AffectedUser == null ? null : new UserSimpleInfo
				{
					Guid = log.AffectedUser.Guid,
					FullName = log.AffectedUser.FullName ?? log.AffectedUser.Login ?? string.Empty,
				},
				AffectedUserGroup = log.AffectedUserGroup == null ? null : new UserGroupSimpleInfo
				{
					Guid = log.AffectedUserGroup.Guid,
					Name = log.AffectedUserGroup.Name,
				},
			})
			.ToArrayAsync(cancellationToken: ct);
	}
}
