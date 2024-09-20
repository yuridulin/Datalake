using Datalake.ApiClasses.Models.Blocks;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class BlocksRepository
{
	public async Task<BlockTreeInfo[]> GetTreeAsync()
	{
		var blocks = await db.Blocks
			.Select(x => new
			{
				x.Id,
				x.Name,
				x.Description,
				x.ParentId,
			})
			.ToArrayAsync();

		return ReadChildren(null);

		BlockTreeInfo[] ReadChildren(int? id)
		{
			return blocks
				.Where(x => x.ParentId == id)
				.Select(x => new BlockTreeInfo
				{
					Id = x.Id,
					Name = x.Name,
					Description = x.Description,
					Children = ReadChildren(x.Id),
				})
				.ToArray();
		}
	}

	public IQueryable<BlockInfo> GetInfoWithAllRelations()
	{
		var query = from block in db.Blocks
								select new BlockInfo
								{
									Id = block.Id,
									Name = block.Name,
									Description = block.Description,
									Parent = (from parent in db.Blocks
														where parent.Id == block.ParentId
														select new BlockInfo.BlockParentInfo
														{
															Id = parent.Id,
															Name = parent.Name
														}).FirstOrDefault(),
									Children = (from child in db.Blocks
															where child.ParentId == block.Id
															select new BlockInfo.BlockChildInfo
															{
																Id = child.Id,
																Name = child.Name
															}).ToArray(),
									Properties = (from property in db.BlockProperties
																where property.BlockId == block.Id
																select new BlockInfo.BlockPropertyInfo
																{
																	Id = property.Id,
																	Name = property.Name,
																	Type = property.Type,
																	Value = property.Value,
																}).ToArray(),
									Tags = (from block_tag in db.BlockTags
													join tag in db.Tags on block_tag.TagId equals tag.Id
													where block_tag.BlockId == block.Id
													select new BlockInfo.BlockTagInfo
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

	public IQueryable<BlockSimpleInfo> GetSimpleInfo()
	{
		return db.Blocks
			.Select(x => new BlockSimpleInfo
			{
				Id = x.Id,
				Name = x.Name,
				Description = x.Description,
			});
	}

	public async Task<List<BlockSimpleInfo>> GetWithParentsAsync(int blockId)
	{
		var blocks = await db.Blocks
			.Select(x => new
			{
				x.Id,
				x.Name,
				x.Description,
				x.ParentId,
			})
			.ToArrayAsync();

		var parents = new List<BlockSimpleInfo>();
		int? seekId = blockId;

		do
		{
			var block = blocks
				.Where(x => x.Id == seekId)
				.FirstOrDefault();

			if (block == null)
				break;

			parents.Add(new BlockSimpleInfo { Name = block.Name, Id = block.Id });
			seekId = block.ParentId;
		}
		while (seekId != null);

		return parents;
	}
}
