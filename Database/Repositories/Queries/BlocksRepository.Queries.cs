using Datalake.ApiClasses.Models.AccessRights;
using Datalake.ApiClasses.Models.Blocks;
using Datalake.ApiClasses.Models.UserGroups;
using Datalake.ApiClasses.Models.Users;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class BlocksRepository
{
	public async Task<BlockTreeInfo[]> GetTreeAsync(Guid? energoId = null)
	{
		// TODO: energoId
		if (energoId.HasValue)
		{ }

		var blocks = await GetSimpleInfo().ToArrayAsync();

		return ReadChildren(null);

		BlockTreeInfo[] ReadChildren(int? id)
		{
			return blocks
				.Where(x => x.ParentId == id)
				.Select(x => new BlockTreeInfo
				{
					Id = x.Id,
					Guid = x.Guid,
					ParentId = x.ParentId,
					Name = x.Name,
					Description = x.Description,
					Tags = x.Tags,
					Children = ReadChildren(x.Id),
				})
				.ToArray();
		}
	}

	public IQueryable<BlockFullInfo> GetInfoWithAllRelations()
	{
		var query =
			from block in db.Blocks
			from parent in db.Blocks.LeftJoin(x => x.Id == block.ParentId)
			select new BlockFullInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				Parent = parent == null ? null : new BlockFullInfo.BlockParentInfo
				{
					Id = parent.Id,
					Name = parent.Name
				},
				Children = 
					from child in db.Blocks.LeftJoin(x => x.ParentId == block.Id)
					select new BlockFullInfo.BlockChildInfo
					{
						Id = child.Id,
						Name = child.Name
					},
				Properties =
					from property in db.BlockProperties.LeftJoin(x => x.BlockId == block.Id)
					select new BlockFullInfo.BlockPropertyInfo
					{
						Id = property.Id,
						Name = property.Name,
						Type = property.Type,
						Value = property.Value,
					},
				Tags = 
					from block_tag in db.BlockTags.InnerJoin(x => x.BlockId == block.Id)
					from tag in db.Tags.LeftJoin(x => x.Id == block_tag.TagId)
					select new BlockNestedTagInfo
					{
						Id = tag.Id,
						Name = block_tag.Name ?? "",
						Guid = tag.GlobalGuid,
						Relation = block_tag.Relation,
						TagName = tag.Name,
						TagType = tag.Type,
					},
				AccessRights = 
					from rights in db.AccessRights.InnerJoin(x => x.BlockId == block.Id)
					from user in db.Users.LeftJoin(x => x.Guid == rights.UserGuid)
					from usergroup in db.UserGroups.LeftJoin(x => x.Guid == rights.UserGroupGuid)
					select new AccessRightsForObjectInfo
					{
						Id = rights.Id,
						IsGlobal = rights.IsGlobal,
						AccessType = rights.AccessType,
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
					},
			};

		return query;
	}

	public IQueryable<BlockWithTagsInfo> GetSimpleInfo(Guid? energoId = null)
	{
		// TODO: energoId
		if (energoId.HasValue)
		{ }

		var query =
			from block in db.Blocks
			select new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				ParentId = block.ParentId,
				Tags = 
					from block_tag in db.BlockTags.InnerJoin(x => x.BlockId == x.TagId)
					from tag in db.Tags.InnerJoin(x => x.Id == block_tag.TagId)
					select new BlockNestedTagInfo
					{
						Id = tag.Id,
						Name = block_tag.Name ?? "",
						Guid = tag.GlobalGuid,
						Relation = block_tag.Relation,
						TagName = tag.Name,
						TagType = tag.Type,
					},
			};

		return query;
	}

	public async Task<List<BlockWithTagsInfo>> GetWithParentsAsync(int blockId)
	{
		var blocks = await db.Blocks
			.Select(x => new
			{
				x.Id,
				x.GlobalId,
				x.Name,
				x.Description,
				x.ParentId,
			})
			.ToArrayAsync();

		var parents = new List<BlockWithTagsInfo>();
		int? seekId = blockId;

		do
		{
			var block = blocks
				.Where(x => x.Id == seekId)
				.FirstOrDefault();

			if (block == null)
				break;

			parents.Add(new BlockWithTagsInfo { Name = block.Name, Id = block.Id, Guid = block.GlobalId });
			seekId = block.ParentId;
		}
		while (seekId != null);

		return parents;
	}
}
