using Datalake.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Database;

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

	public virtual DbSet<TagInput> TagInputs { get; set; }

	public virtual DbSet<User> Users { get; set; }

	public virtual DbSet<UserGroup> UserGroups { get; set; }

	public virtual DbSet<UserGroupRelation> UserGroupRelations { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("public");

		// связь блоков по иерархии
		modelBuilder.Entity<Block>()
			.HasOne(block => block.Parent)
			.WithMany(block => block.Children)
			.HasForeignKey(block => block.ParentId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь блоков и тегов
		modelBuilder.Entity<Block>()
			.HasMany(block => block.Tags)
			.WithMany(tag => tag.Blocks)
			.UsingEntity<BlockTag>(
				relation => relation
					.HasOne(rel => rel.Tag)
					.WithMany(tag => tag.RelationsToBlocks)
					.HasForeignKey(rel => rel.TagId)
					.OnDelete(DeleteBehavior.SetNull),
				relation => relation
					.HasOne(rel => rel.Block)
					.WithMany(e => e.RelationsToTags)
					.HasForeignKey(rel => rel.BlockId)
					.OnDelete(DeleteBehavior.Cascade),
				relation => relation
					.HasKey(rel => new { rel.BlockId, rel.TagId })
			);

		// связь групп пользователей по иерархии
		modelBuilder.Entity<UserGroup>()
			.HasOne(group => group.Parent)
			.WithMany(group => group.Children)
			.HasForeignKey(group => group.ParentGuid)
			.OnDelete(DeleteBehavior.SetNull);

		// связь пользователей и групп пользователей
		modelBuilder
			.Entity<UserGroup>()
			.HasMany(group => group.Users)
			.WithMany(user => user.Groups)
			.UsingEntity<UserGroupRelation>(
				relation => relation
					.HasOne(rel => rel.User)
					.WithMany(user => user.GroupsRelations)
					.HasForeignKey(rel => rel.UserGuid)
					.OnDelete(DeleteBehavior.Cascade),
				relation => relation
					.HasOne(rel => rel.UserGroup)
					.WithMany(group => group.UsersRelations)
					.HasForeignKey(rel => rel.UserGroupGuid)
					.OnDelete(DeleteBehavior.Cascade),
				relation => relation
					.HasKey(rel => new { rel.UserGroupGuid, rel.UserGuid })
			);

		// связь источников и тегов
		modelBuilder
			.Entity<Tag>()
			.HasOne(tag => tag.Source)
			.WithMany(source => source.Tags)
			.HasForeignKey(tag => tag.SourceId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь тегов с входными тегами (переменными)
		modelBuilder.Entity<Tag>()
			.HasMany(tag => tag.Inputs)
			.WithOne(input => input.Tag)
			.HasForeignKey(input => input.TagId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<TagInput>()
			.HasOne(input => input.InputTag)
			.WithMany()
			.HasForeignKey(input => input.InputTagId)
			.OnDelete(DeleteBehavior.SetNull);

		// связи модели прав с объектами
		modelBuilder.Entity<AccessRights>()
			.HasOne(x => x.Block)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.BlockId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AccessRights>()
			.HasOne(x => x.Source)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.SourceId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AccessRights>()
			.HasOne(x => x.Tag)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.TagId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AccessRights>()
			.HasOne(x => x.User)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.UserGuid)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<AccessRights>()
			.HasOne(x => x.UserGroup)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.UserGroupGuid)
			.OnDelete(DeleteBehavior.Cascade);
	}

	/// <summary>
	/// Сообщение аудита в БД
	/// </summary>
	/// <param name="db"></param>
	/// <param name="log"></param>
	/// <returns></returns>
	public async Task LogAsync(Log log)
	{
		try
		{
			await Logs.AddAsync(log);
		}
		catch { }
	}
}
