using Datalake.Inventory.Constants;
using Datalake.InventoryService.Database.Tables;
using Datalake.InventoryService.Database.Views;
using Datalake.PrivateApi.Settings;
using Datalake.PublicApi.Models.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Datalake.InventoryService.Database;

/// <summary>
/// Контекст базы данных EF, используется для миграций
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

		// настройка тегов

		modelBuilder.Entity<Tag>()
			.Property(t => t.Thresholds)
			.HasColumnType("jsonb")
			.HasConversion(
				v => JsonSerializer.Serialize(v, JsonSettings.JsonSerializerOptions),
				v => JsonSerializer.Deserialize<List<TagThresholdInfo>>(v, JsonSettings.JsonSerializerOptions)
			)
			.Metadata.SetValueComparer(
				new ValueComparer<List<TagThresholdInfo>>(
					(a, b) => Equals(a, b),
					v => v == null ? 0 : v.GetHashCode(),
					v => v == null ? null! : new List<TagThresholdInfo>(v)
				)
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

		// связь тегов с входными тегами для агрегирования

		modelBuilder.Entity<Tag>()
			.HasOne(tag => tag.SourceTag)
			.WithMany()
			.HasForeignKey(tag => tag.SourceTagId)
			.OnDelete(DeleteBehavior.SetNull);

		// связь тегов с входными тегами для расчета

		modelBuilder.Entity<Tag>()
			.HasOne(tag => tag.ThresholdSourceTag)
			.WithMany()
			.HasForeignKey(tag => tag.ThresholdSourceTagId)
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

		// связь логов с пользователем, чьи действия записаны

		modelBuilder.Entity<User>()
			.HasMany(x => x.Actions)
			.WithOne(x => x.Author)
			.HasForeignKey(x => x.AuthorGuid)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<Log>()
			.HasOne(x => x.AffectedSource)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedSourceId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<Log>()
			.HasOne(x => x.AffectedBlock)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedBlockId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<Log>()
			.HasOne(x => x.AffectedTag)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedTagId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<Log>()
			.HasOne(x => x.AffectedUser)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedUserGuid)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<Log>()
			.HasOne(x => x.AffectedUserGroup)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedUserGroupGuid)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<Log>()
			.HasOne(x => x.AffectedAccessRights)
			.WithMany(x => x.Logs)
			.HasForeignKey(x => x.AffectedAccessRightsId)
			.OnDelete(DeleteBehavior.SetNull);

		// представление для пользователей EnergoId

		modelBuilder.Entity<EnergoIdUserView>()
			.ToTable(name: null)
			.ToView(EnergoIdDefinitions.UsersView.ViewName, schema: EnergoIdDefinitions.Schema);

		modelBuilder.Entity<EnergoIdUserView>()
			.HasOne(x => x.User)
			.WithOne(x => x.EnergoId)
			.HasForeignKey<User>(x => x.EnergoIdGuid)
			.HasPrincipalKey<EnergoIdUserView>(x => x.Guid);

		// сессии

		modelBuilder.Entity<UserSession>()
			.HasOne(x => x.User)
			.WithMany(x => x.Sessions)
			.HasForeignKey(x => x.UserGuid)
			.HasPrincipalKey(x => x.Guid)
			.OnDelete(DeleteBehavior.Cascade);
	}

	#region Таблицы

	/// <summary>
	/// Таблица прав доступа
	/// </summary>
	public virtual DbSet<AccessRights> AccessRights { get; set; }

	/// <summary>
	/// Таблица блоков
	/// </summary>
	public virtual DbSet<Block> Blocks { get; set; }

	/// <summary>
	/// Таблица статичный свойств блоков
	/// </summary>
	public virtual DbSet<BlockProperty> BlockProperties { get; set; }

	/// <summary>
	/// Таблица связей блоков и тегов
	/// </summary>
	public virtual DbSet<BlockTag> BlockTags { get; set; }

	/// <summary>
	/// Таблица логов аудита
	/// </summary>
	public virtual DbSet<Log> Logs { get; set; }

	/// <summary>
	/// Таблица настроек
	/// </summary>
	public virtual DbSet<Settings> Settings { get; set; }

	/// <summary>
	/// Таблица источников
	/// </summary>
	public virtual DbSet<Source> Sources { get; set; }

	/// <summary>
	/// Таблица тегов
	/// </summary>
	public virtual DbSet<Tag> Tags { get; set; }

	/// <summary>
	/// Таблица входных параметров для вычисляемых тегов
	/// </summary>
	public virtual DbSet<TagInput> TagInputs { get; set; }

	/// <summary>
	/// Таблица пользователей
	/// </summary>
	public virtual DbSet<User> Users { get; set; }

	/// <summary>
	/// Таблица групп пользователей
	/// </summary>
	public virtual DbSet<UserGroup> UserGroups { get; set; }

	/// <summary>
	/// Таблица связей пользователей и групп пользователей
	/// </summary>
	public virtual DbSet<UserGroupRelation> UserGroupRelations { get; set; }

	/// <summary>
	/// Представление пользователей EnergoId с их данными
	/// </summary>
	public virtual DbSet<EnergoIdUserView> UsersEnergoId { get; set; }

	/// <summary>
	/// Таблица текущих сессий пользователей
	/// </summary>
	public virtual DbSet<UserSession> UserSessions { get; set; }

	#endregion
}
