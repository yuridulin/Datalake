using DatalakeDatabase.ApiModels.Blocks;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Models;
using LinqToDB;

namespace DatalakeDatabase.Repositories
{
	public partial class BlocksRepository
	{
		public async Task<Block[]> GetAsTree()
		{
			var blocks = await db.Blocks
				.ToArrayAsync();

			var top = blocks
				.Where(x => x.ParentId == null)
				.ToArray();

			foreach (var entity in top)
			{
				entity.Children = ReadChildren(entity.Id);
			}

			return blocks;

			Block[] ReadChildren(int id)
			{
				var children = blocks.Where(x => x.ParentId == id).ToArray();

				foreach (var child in children)
				{
					child.Children = ReadChildren(child.Id);
				}

				return children;
			}
		}

		public async Task<BlockInfo> GetAsync(int id)
		{
			var query = from block in db.Blocks.Where(x => x.Id == id)
									from property in db.BlockProperties.LeftJoin(x => x.BlockId == block.Id)
									from child in db.Blocks.LeftJoin(x => x.ParentId == block.Id)
									from parent in db.Blocks.LeftJoin(x => x.Id == block.ParentId)
									from block_tag in db.BlockTags.LeftJoin(x => x.BlockId == block.Id)
									from tag in db.Tags.InnerJoin(x => x.Id == block_tag.TagId)
									select new
									{
										block,
										parent,
										child,
										property,
										block_tag,
										tag
									};

			var raw = await query
				.ToListAsync();

			var info = raw
				.GroupBy(x => x.block)
				.Select(g => new BlockInfo
				{
					Id = g.Key.Id,
					Name = g.Key.Name,
					Description = g.Key.Description,
					Parent = g.Select(x => x.parent)
						.Select(x => new BlockInfo.BlockParentInfo { Id = x.Id, Name = x.Name })
						.FirstOrDefault(),
					Children = g.Select(x => x.child)
						.Select(x => new BlockInfo.BlockChildInfo { Id = x.Id, Name = x.Name })
						.ToArray(),
					Properties = g.Select(x => x.property)
						.Select(x => new BlockInfo.BlockPropertyInfo { Id = x.Id, Name = x.Name })
						.ToArray(),
					Tags = g
						.Select(x => new BlockInfo.BlockTagInfo { Id = x.tag.Id, Name = x.block_tag.Name ?? "" })
						.ToArray(),
				})
				.FirstOrDefault()
				?? throw new NotFoundException($"Сущность #{id} не найдена");

			return info;
		}
	}
}
