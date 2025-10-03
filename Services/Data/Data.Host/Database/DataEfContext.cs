using Datalake.Data.Host.Database.Constants;
using Datalake.Data.Host.Database.Entities;
using Datalake.Shared.Application;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Data.Host.Database;

[Scoped]
public class DataEfContext(DbContextOptions<DataEfContext> options) : DbContext(options)
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(DataDefinition.Schema);

		modelBuilder.Entity<TagHistory>(tagHistory =>
		{
			tagHistory.ToTable(DataDefinition.TagHistory.Table);
			tagHistory.HasNoKey();

			tagHistory.Property(x => x.TagId).HasColumnName(DataDefinition.TagHistory.TagId);
			tagHistory.Property(x => x.Date).HasColumnName(DataDefinition.TagHistory.Date);
			tagHistory.Property(x => x.Text).HasColumnName(DataDefinition.TagHistory.Text);
			tagHistory.Property(x => x.Number).HasColumnName(DataDefinition.TagHistory.Number);
			tagHistory.Property(x => x.Quality).HasColumnName(DataDefinition.TagHistory.Quality);

			// уникальность значений тегов на дату
			tagHistory
				.HasIndex(record => new { record.TagId, record.Date })
				.HasDatabaseName(DataDefinition.TagHistory.UniqueIndexName)
				.IsDescending([false, true])
				.IsUnique();
		});
	}

	/// <summary>
	/// Гипер-таблица архивных значений тегов
	/// </summary>
	public virtual DbSet<TagHistory> TagsHistory { get; set; }
}
