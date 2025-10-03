using Datalake.InventoryService.Domain.Entities;

namespace Datalake.GatewayService;

public class GatewayContext : DbContext(options)
{
	/// <summary>
	/// Конфигурация связей между таблицами БД
	/// </summary>
	/// <param name="modelBuilder"></param>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("gateway");

		// сессии

		modelBuilder.Entity<UserSession>()
			.HasOne(x => x.User)
			.WithMany(x => x.Sessions)
			.HasForeignKey(x => x.UserGuid)
			.HasPrincipalKey(x => x.Guid)
			.OnDelete(DeleteBehavior.Cascade);
	}


	/// <summary>
	/// Таблица текущих сессий пользователей
	/// </summary>
	public virtual DbSet<UserSession> UserSessions { get; set; }
}
