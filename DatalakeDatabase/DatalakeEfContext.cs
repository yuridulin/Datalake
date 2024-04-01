using DatalakeDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace DatalakeDatabase
{
	public class DatalakeEfContext(DbContextOptions<DatalakeEfContext> options) : DbContext(options)
	{
		public virtual DbSet<Block> Blocks { get; set; }

		public virtual DbSet<Log> Logs { get; set; }

		public virtual DbSet<RelBlockTag> RelBlockTags { get; set; }

		public virtual DbSet<RelTagInput> RelTagInputs { get; set; }

		public virtual DbSet<Setting> Settings { get; set; }

		public virtual DbSet<Source> Sources { get; set; }

		public virtual DbSet<Tag> Tags { get; set; }

		public virtual DbSet<User> Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Log>(entity =>
			{
				entity.HasNoKey();

				entity.Property(e => e.Date).HasColumnType("timestamp without time zone");
			});

			modelBuilder.Entity<RelBlockTag>(entity =>
			{
				entity
						.HasNoKey()
						.ToTable("Rel_Block_Tag");
			});

			modelBuilder.Entity<RelTagInput>(entity =>
			{
				entity
						.HasNoKey()
						.ToTable("Rel_Tag_Input");
			});

			modelBuilder.Entity<Setting>(entity =>
			{
				entity.HasNoKey();
			});

			modelBuilder.Entity<Source>(entity =>
			{
				entity.HasNoKey();

				entity.Property(e => e.Id).ValueGeneratedOnAdd();
			});

			modelBuilder.Entity<Tag>(entity =>
			{
				entity.Property(e => e.MaxEu).HasColumnName("MaxEU");
				entity.Property(e => e.MinEu).HasColumnName("MinEU");
			});

			modelBuilder.Entity<User>(entity =>
			{
				entity.HasNoKey();
			});
		}
	}
}
