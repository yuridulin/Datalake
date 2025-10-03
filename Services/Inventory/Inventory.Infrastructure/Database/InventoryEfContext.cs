using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Datalake.Inventory.Infrastructure.Database;

/// <summary>
/// Контекст базы данных EF
/// </summary>
public class InventoryEfContext(DbContextOptions<InventoryEfContext> options) : DbContext(options)
{
	/// <summary>
	/// Конфигурация связей между таблицами БД
	/// </summary>
	/// <param name="modelBuilder"></param>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("public");

		// Автоматически применяет все классы конфигурации
		// из текущей сборки, реализующие IEntityTypeConfiguration<>
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		// связь блоков по иерархии

		modelBuilder.Entity<BlockEntity>()
			.HasOne(block => block.Parent)
			.WithMany(block => block.Children)
			.HasForeignKey(block => block.ParentId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь блоков и тегов

		modelBuilder.Entity<BlockEntity>()
			.HasMany(block => block.Tags)
			.WithMany(tag => tag.Blocks)
			.UsingEntity<BlockTagEntity>(
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

		modelBuilder.Entity<UserGroupEntity>()
			.HasOne(group => group.Parent)
			.WithMany(group => group.Children)
			.HasForeignKey(group => group.ParentGuid)
			.OnDelete(DeleteBehavior.SetNull);

		// связь пользователей и групп пользователей

		modelBuilder
			.Entity<UserGroupEntity>()
			.HasMany(group => group.Users)
			.WithMany(user => user.Groups)
			.UsingEntity<UserGroupRelationEntity>(
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
			.Entity<TagEntity>()
			.HasOne(tag => tag.Source)
			.WithMany(source => source.Tags)
			.HasForeignKey(tag => tag.SourceId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь тегов с входными тегами (переменными)

		modelBuilder.Entity<TagEntity>()
			.HasMany(tag => tag.Inputs)
			.WithOne(input => input.Tag)
			.HasForeignKey(input => input.TagId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<TagInputEntity>()
			.HasOne(input => input.InputTag)
			.WithMany()
			.HasForeignKey(input => input.InputTagId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь тегов с входными тегами для агрегирования

		modelBuilder.Entity<TagEntity>()
			.HasOne(tag => tag.SourceTag)
			.WithMany()
			.HasForeignKey(tag => tag.SourceTagId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь тегов с входными тегами для расчета

		modelBuilder.Entity<TagEntity>()
			.HasOne(tag => tag.ThresholdSourceTag)
			.WithMany()
			.HasForeignKey(tag => tag.ThresholdSourceTagId)
			.OnDelete(DeleteBehavior.SetNull);

		// связи модели прав с объектами

		modelBuilder.Entity<AccessRuleEntity>()
			.HasOne(x => x.Block)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.BlockId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AccessRuleEntity>()
			.HasOne(x => x.Source)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.SourceId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AccessRuleEntity>()
			.HasOne(x => x.Tag)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.TagId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AccessRuleEntity>()
			.HasOne(x => x.User)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.UserGuid)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<AccessRuleEntity>()
			.HasOne(x => x.UserGroup)
			.WithMany(x => x.AccessRightsList)
			.HasForeignKey(x => x.UserGroupGuid)
			.OnDelete(DeleteBehavior.Cascade);

		// связь логов с пользователем, чьи действия записаны

		modelBuilder.Entity<UserEntity>()
			.HasMany(x => x.Actions)
			.WithOne(x => x.Author)
			.HasForeignKey(x => x.AuthorGuid)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AuditEntity>()
			.HasOne(x => x.AffectedSource)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedSourceId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AuditEntity>()
			.HasOne(x => x.AffectedBlock)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedBlockId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AuditEntity>()
			.HasOne(x => x.AffectedTag)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedTagId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AuditEntity>()
			.HasOne(x => x.AffectedUser)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedUserGuid)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AuditEntity>()
			.HasOne(x => x.AffectedUserGroup)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedUserGroupGuid)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<AuditEntity>()
			.HasOne(x => x.AffectedAccessRights)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedAccessRightsId)
			.OnDelete(DeleteBehavior.SetNull);

		// представление для пользователей EnergoId

		modelBuilder.Entity<EnergoIdEntity>()
			.ToTable(name: null)
			.ToView(EnergoIdDefinitions.UsersView.ViewName, schema: EnergoIdDefinitions.Schema);

		modelBuilder.Entity<EnergoIdEntity>()
			.HasOne(x => x.User)
			.WithOne(x => x.EnergoId)
			.HasForeignKey<UserEntity>(x => x.EnergoIdGuid)
			.HasPrincipalKey<EnergoIdEntity>(x => x.Guid);
	}

	#region Таблицы

	/// <summary>
	/// Таблица прав доступа
	/// </summary>
	public virtual DbSet<AccessRuleEntity> AccessRights { get; set; }

	/// <summary>
	/// Таблица блоков
	/// </summary>
	public virtual DbSet<BlockEntity> Blocks { get; set; }

	/// <summary>
	/// Таблица статичный свойств блоков
	/// </summary>
	public virtual DbSet<BlockPropertyEntity> BlockProperties { get; set; }

	/// <summary>
	/// Таблица связей блоков и тегов
	/// </summary>
	public virtual DbSet<BlockTagEntity> BlockTags { get; set; }

	/// <summary>
	/// Таблица логов аудита
	/// </summary>
	public virtual DbSet<AuditEntity> Audit { get; set; }

	/// <summary>
	/// Таблица настроек
	/// </summary>
	public virtual DbSet<SettingsEntity> Settings { get; set; }

	/// <summary>
	/// Таблица источников
	/// </summary>
	public virtual DbSet<SourceEntity> Sources { get; set; }

	/// <summary>
	/// Таблица тегов
	/// </summary>
	public virtual DbSet<TagEntity> Tags { get; set; }

	/// <summary>
	/// Таблица входных параметров для вычисляемых тегов
	/// </summary>
	public virtual DbSet<TagInputEntity> TagInputs { get; set; }

	/// <summary>
	/// Таблица порогов для пороговых тегов
	/// </summary>
	public virtual DbSet<TagThresholdEntity> TagThresholds { get; set; }

	/// <summary>
	/// Таблица пользователей
	/// </summary>
	public virtual DbSet<UserEntity> Users { get; set; }

	/// <summary>
	/// Таблица групп пользователей
	/// </summary>
	public virtual DbSet<UserGroupEntity> UserGroups { get; set; }

	/// <summary>
	/// Таблица связей пользователей и групп пользователей
	/// </summary>
	public virtual DbSet<UserGroupRelationEntity> UserGroupRelations { get; set; }

	/// <summary>
	/// Представление пользователей EnergoId с их данными
	/// </summary>
	public virtual DbSet<EnergoIdEntity> EnergoIdView { get; set; }

	#endregion
}
