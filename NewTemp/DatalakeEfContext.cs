using DatalakeDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace DatalakeDatabase
{
	public partial class DatalakeEfContext(DbContextOptions<DatalakeEfContext> options) : DbContext(options)
	{
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder
				.Entity<Entity>()
				.HasMany(e => e.Tags)
				.WithMany(t => t.Entities)
				.UsingEntity<EntityTag>(
					r => r
					.HasOne(x => x.Tag)
					.WithMany(t => t.RelatedEntities)
					.HasForeignKey(x => x.TagId),
					r => r
					.HasOne(x => x.Entity)
					.WithMany(e => e.RelatedTags)
					.HasForeignKey(x => x.EntityId),
					r => r
					.HasKey(x => new { x.EntityId, x.TagId })
				);
		}

		#region Таблицы

		public DbSet<Tag> Tags { get; set; }

		public DbSet<TagHistory> TagsLive { get; set; }

		public DbSet<TagHistoryChunk> Chunks { get; set; }

		public DbSet<Source> Sources { get; set; }

		public DbSet<Entity> Entities { get; set; }

		public DbSet<EntityField> EntityFields { get; set; }

		public DbSet<EntityTag> EntityTags { get; set; }

		public DbSet<TagInput> TagInputs { get; set; }

		public DbSet<Settings> Settings { get; set; }

		#endregion
	}
}
