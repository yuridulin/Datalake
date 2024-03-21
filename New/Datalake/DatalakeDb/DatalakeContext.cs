using DatalakeDb.Models;
using Microsoft.EntityFrameworkCore;

namespace DatalakeDb
{
	public class DatalakeContext : DbContext
	{
		public DatalakeContext(DbContextOptions<DatalakeContext> options) : base(options)
		{
			Database.EnsureCreated();
		}

		public DbSet<Tag> Tags { get; set; }

		/* 
		 * А тут мы вдруг выясняем, что EF Core не хочет позволять выбирать таблицу, из которой брать данные.
		 * Уточнить.
		 * Если это так, будем сверху прикручивать linq2db (хвала создателям за её создание)
		 */
		public DbSet<History> History { get; set; }

		public DbSet<Source> Sources { get; set; }

		public DbSet<Entity> Entities { get; set; }

		public DbSet<EntityField> EntityFields { get; set; }

		public DbSet<EntityTag> EntityTags { get; set; }

		public DbSet<TagInput> TagInputs { get; set; }

		/*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);

			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.SetBasePath(Directory.GetCurrentDirectory())
				.Build();

			optionsBuilder.UseNpgsql(config.GetConnectionString("Default"));
		}*/
	}
}
