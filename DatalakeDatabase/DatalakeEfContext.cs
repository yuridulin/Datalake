using DatalakeDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace DatalakeDatabase;

public class DatalakeEfContext(DbContextOptions<DatalakeEfContext> options) : DbContext(options)
{
	public virtual DbSet<Block> Blocks { get; set; }

	public virtual DbSet<BlockProperty> BlockProperties { get; set; }

	public virtual DbSet<BlockTag> BlockTags { get; set; }

	public virtual DbSet<Settings> Settings { get; set; }

	public virtual DbSet<Source> Sources { get; set; }

	public virtual DbSet<Tag> Tags { get; set; }

	public virtual DbSet<TagHistoryChunk> TagHistoryChunks { get; set; }

	public virtual DbSet<TagInput> TagInputs { get; set; }

	public virtual DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder
			.Entity<Block>()
			.HasMany(e => e.Tags)
			.WithMany(t => t.Blocks)
			.UsingEntity<BlockTag>(
				r => r
				.HasOne(x => x.Tag)
				.WithMany(t => t.RelationsToBlocks)
				.HasForeignKey(x => x.TagId)
				.OnDelete(DeleteBehavior.NoAction),
				r => r
				.HasOne(x => x.Block)
				.WithMany(e => e.RelationsToTags)
				.HasForeignKey(x => x.BlockId)
				.OnDelete(DeleteBehavior.NoAction),
				r => r
				.HasKey(x => new { x.BlockId, x.TagId })
			);
	}
}
