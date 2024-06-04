using DatalakeApiClasses.Models.Blocks;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

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
			;
		}
	}

	public IQueryable<BlockInfo> GetInfoWithAllRelations()
	{
		var query = from block in db.Blocks
								from property in db.BlockProperties.LeftJoin(x => x.BlockId == block.Id)
								from child in db.Blocks.LeftJoin(x => x.ParentId == block.Id)
								from parent in db.Blocks.LeftJoin(x => x.Id == block.ParentId)
								from block_tag in db.BlockTags.LeftJoin(x => x.BlockId == block.Id)
								from tag in db.Tags.InnerJoin(x => x.Id == block_tag.TagId)
								group new
								{
									block,
									parent,
									child,
									property,
									block_tag,
									tag
								}
								by block into g
								select new BlockInfo
								{
									Id = g.Key.Id,
									Name = g.Key.Name,
									Description = g.Key.Description,
									Parent = g.Select(x => x.parent)
										.Select(x => new BlockInfo.BlockParentInfo
										{
											Id = x.Id,
											Name = x.Name
										})
										.FirstOrDefault(),
									Children = g.Select(x => x.child)
										.Select(x => new BlockInfo.BlockChildInfo
										{
											Id = x.Id,
											Name = x.Name
										})
										.ToArray(),
									Properties = g.Select(x => x.property)
										.Select(x => new BlockInfo.BlockPropertyInfo
										{
											Id = x.Id,
											Name = x.Name,
											Type = x.Type,
											Value = x.Value,
										})
										.ToArray(),
									Tags = g
										.Select(x => new BlockInfo.BlockTagInfo
										{
											Id = x.tag.Id,
											Name = x.block_tag.Name ?? "",
											TagType = x.block_tag.Relation,
										})
										.ToArray(),
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

			if (block == null) break;

			parents.Add(new BlockSimpleInfo { Name = block.Name, Id = block.Id });
			seekId = block.ParentId;
		}
		while (seekId != null);

		return parents;
	}
}
