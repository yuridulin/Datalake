using Datalake.ApiClasses.Models.AccessRights;
using Datalake.ApiClasses.Models.Blocks;
using Datalake.ApiClasses.Models.Sources;
using Datalake.ApiClasses.Models.Tags;
using Datalake.ApiClasses.Models.UserGroups;
using Datalake.ApiClasses.Models.Users;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class AccessRepository
{
	public IQueryable<AccessRightsInfo> GetAccessRightsInfo(
		Guid? userGuid = null,
		Guid? userGroupGuid = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null)
	{
		var rightsQuery = db.AccessRights
			.Where(x => userGuid == null || x.UserGuid == userGuid)
			.Where(x => userGroupGuid == null || x.UserGroupGuid == userGroupGuid)
			.Where(x => sourceId == null || x.SourceId == sourceId)
			.Where(x => blockId == null || x.BlockId == blockId)
			.Where(x => tagId == null || x.TagId == tagId);

		var query =
			from rights in rightsQuery
			from user in db.Users.LeftJoin(x => x.Guid == rights.UserGuid)
			from usergroup in db.UserGroups.LeftJoin(x => x.Guid == rights.UserGroupGuid)
			from source in db.Sources.LeftJoin(x => x.Id == rights.SourceId)
			from block in db.Blocks.LeftJoin(x => x.Id == rights.BlockId)
			from tag in db.Tags.LeftJoin(x => x.Id == rights.TagId)
			select new AccessRightsInfo
			{
				Id = rights.Id,
				AccessType = rights.AccessType,
				IsGlobal = rights.IsGlobal,
				Source = source == null ? null : new SourceSimpleInfo
				{
					Id = source.Id,
					Name = source.Name,
				},
				User = user == null ? null : new UserSimpleInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? string.Empty,
				},
				UserGroup = usergroup == null ? null : new UserGroupSimpleInfo
				{
					Guid = usergroup.Guid,
					Name = usergroup.Name,
				},
				Block = block == null ? null : new BlockSimpleInfo
				{
					Id = block.Id,
					Guid = block.GlobalId,
					Name = block.Name,
				},
				Tag = tag == null ? null : new TagSimpleInfo
				{
					Id = tag.Id,
					Guid = tag.GlobalGuid,
					Name = tag.Name,
				},
			};

		return query;
	}
}
