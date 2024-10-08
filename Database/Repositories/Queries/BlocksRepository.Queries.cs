using Datalake.ApiClasses.Models.Blocks;
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
		var query = from block in db.Blocks
								select new BlockFullInfo
								{
									Id = block.Id,
									Guid = block.GlobalId,
									Name = block.Name,
									Description = block.Description,
									Parent = (from parent in db.Blocks
														where parent.Id == block.ParentId
														select new BlockFullInfo.BlockParentInfo
														{
															Id = parent.Id,
															Name = parent.Name
														}).FirstOrDefault(),
									Children = (from child in db.Blocks
															where child.ParentId == block.Id
															select new BlockFullInfo.BlockChildInfo
															{
																Id = child.Id,
																Name = child.Name
															}).ToArray(),
									Properties = (from property in db.BlockProperties
																where property.BlockId == block.Id
																select new BlockFullInfo.BlockPropertyInfo
																{
																	Id = property.Id,
																	Name = property.Name,
																	Type = property.Type,
																	Value = property.Value,
																}).ToArray(),
									Tags = (from block_tag in db.BlockTags
													join tag in db.Tags on block_tag.TagId equals tag.Id
													where block_tag.BlockId == block.Id
													select new BlockNestedTagInfo
													{
														Id = tag.Id,
														Name = block_tag.Name ?? "",
														Guid = tag.GlobalGuid,
														Relation = block_tag.Relation,
														TagName = tag.Name,
														TagType = tag.Type,
													}).ToArray(),
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
				Tags = (from block_tag in db.BlockTags
								join tag in db.Tags on block_tag.TagId equals tag.Id
								where block_tag.BlockId == block.Id
								select new BlockNestedTagInfo
								{
									Id = tag.Id,
									Name = block_tag.Name ?? "",
									Guid = tag.GlobalGuid,
									Relation = block_tag.Relation,
									TagName = tag.Name,
									TagType = tag.Type,
								}).ToArray(),
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
