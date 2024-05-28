using DatalakeDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace DatalakeDatabase;

public class DatalakeEfContext(DbContextOptions<DatalakeEfContext> options) : DbContext(options)
{
	public virtual DbSet<AccessRights> AccessRights { get; set; }

	public virtual DbSet<Block> Blocks { get; set; }

	public virtual DbSet<BlockProperty> BlockProperties { get; set; }

	public virtual DbSet<BlockTag> BlockTags { get; set; }

	public virtual DbSet<Log> Logs { get; set; }

	public virtual DbSet<Settings> Settings { get; set; }

	public virtual DbSet<Source> Sources { get; set; }

	public virtual DbSet<Tag> Tags { get; set; }

	public virtual DbSet<TagHistoryChunk> TagHistoryChunks { get; set; }

	public virtual DbSet<TagInput> TagInputs { get; set; }

	public virtual DbSet<TagHistory> TagsLive { get; set; }

	public virtual DbSet<User> Users { get; set; }

	public virtual DbSet<UserGroup> UserGroups { get; set; }

	public virtual DbSet<UserGroupRelation> UserGroupRelations { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// связь объектов и тегов
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

		// связь пользователей и групп пользователей
		modelBuilder
			.Entity<UserGroup>()
			.HasMany(e => e.Users)
			.WithMany(t => t.Groups)
			.UsingEntity<UserGroupRelation>(
				r => r
				.HasOne(x => x.User)
				.WithMany(t => t.GroupsRelations)
				.HasForeignKey(x => x.UserGroupGuid)
				.OnDelete(DeleteBehavior.NoAction),
				r => r
				.HasOne(x => x.UserGroup)
				.WithMany(e => e.UsersRelations)
				.HasForeignKey(x => x.UserGuid)
				.OnDelete(DeleteBehavior.NoAction),
				r => r
				.HasKey(x => new { x.UserGroupGuid, x.UserGuid })
			);
	}
}
