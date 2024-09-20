using Datalake.ApiClasses.Models.Blocks;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Database.Repositories;

public partial class BlocksRepository
{
	public async Task<BlockTreeInfo[]> GetTreeAsync()
	{
		var blocks = await db.Blocks
			.AsNoTracking()
			.Select(x => new
			{
				x.Id,
				x.Name,
				x.Description,
				x.ParentId,
			})
			.ToArrayAsyncEF();

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
		var query = db.Blocks
			.Include(x => x.Properties)
			.Include(x => x.Children)
			.Include(x => x.Parent)
			.Include(x => x.RelationsToTags)
			.ThenInclude(x => x.Tag)
			.AsNoTracking()
			.Select(x => new BlockInfo
			{
				Id = x.Id,
				Name = x.Name,
				Description = x.Description,
				Properties = x.Properties
					.Select(p => new BlockInfo.BlockPropertyInfo
					{
						Id = p.Id,
						Name = p.Name,
						Type = p.Type,
						Value = p.Value,
					})
					.ToArray(),
				Parent = x.Parent == null
					? null
					: new BlockInfo.BlockParentInfo
					{
						Id = x.Parent.Id,
						Name = x.Parent.Name,
					},
				Children = x.Children
					.Select(c => new BlockInfo.BlockChildInfo
					{
						Id = c.Id,
						Name = c.Name,
					})
					.ToArray(),
				Tags = x.RelationsToTags
					.Where(r => r.Tag != null)
					.Select(r => new BlockInfo.BlockTagInfo
					{
						Guid = r.Tag!.GlobalGuid,
						Name = r.Name ?? "",
						Id = r.Tag!.Id,
						TagName = r.Tag!.Name,
						TagType = r.Tag!.Type,
					})
					.ToArray(),
			});

		return query;
	}

	public IQueryable<BlockSimpleInfo> GetSimpleInfo()
	{
		return db.Blocks
			.AsNoTracking()
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
			.AsNoTracking()
			.Select(x => new
			{
				x.Id,
				x.Name,
				x.Description,
				x.ParentId,
			})
			.ToArrayAsyncEF();

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

	protected async Task<BlockSimpleInfo[]> GetParentsFlat(int? id)
	{
		if (id == null)
			return [];

		var linqToDb = db.CreateLinqToDBConnection();

		var recCTE = linqToDb.GetCte<BlockSimpleInfo>(cte => (
			from b in db.Blocks
			where b.Id == id
			select new BlockSimpleInfo
			{
				Id = b.Id,
				Name = b.Name,
				ParentId = b.ParentId
			})
			.Concat(
					from r in cte
					join b in db.Blocks on r.ParentId equals b.Id
					select new BlockSimpleInfo
					{
						Id = b.Id,
						Name = b.Name,
						ParentId = b.ParentId
					}
			)
		);

		var result = await recCTE.ToArrayAsyncLinqToDB();

		return result;
	}

	protected async Task<BlockSimpleInfo[]> GetChildsFlat(int? id)
	{
		if (id == null)
			return await GetSimpleInfo().ToArrayAsyncEF();

		var linqToDb = db.CreateLinqToDBConnection();

		var recCTE = linqToDb.GetCte<BlockSimpleInfo>(cte => (
			from b in db.Blocks
			where b.ParentId == id
			select new BlockSimpleInfo
			{
				Id = b.Id,
				Name = b.Name,
				ParentId = b.ParentId
			})
			.Concat(
					from r in cte
					join b in db.Blocks on r.Id equals b.ParentId
					select new BlockSimpleInfo
					{
						Id = b.Id,
						Name = b.Name,
						ParentId = b.ParentId
					}
			)
		);

		var result = await recCTE.ToArrayAsyncLinqToDB();

		return result;
	}
}
