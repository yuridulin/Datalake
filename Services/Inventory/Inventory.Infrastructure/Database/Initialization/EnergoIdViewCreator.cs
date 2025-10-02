using Datalake.Inventory.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Schema;
using Datalake.Inventory.Infrastructure.Interfaces;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Datalake.Inventory.Infrastructure.Database.Initialization;

/// <summary>
/// Актуализация подключения к внешней БД keycloak - "EnergoID"
/// </summary>
[Scoped]
public class EnergoIdViewCreator(
	NpgsqlDataSource dataSource,
	IConfiguration configuration,
	ILogger<EnergoIdViewCreator> logger) : IEnergoIdViewCreator
{
	/// <summary>
	/// Актуализация подключения к внешней БД keycloak - "EnergoID"
	/// </summary>
	public async Task RecreateAsync(CancellationToken ct = default)
	{
		logger.LogInformation("Актуализация подключения к внешней БД EnergoId");

		// чтение настроек из конфига
		var settings = configuration.GetSection("ExternalDb").Get<ExternalDbOptions>();
		if (settings == null)
		{
			logger.LogWarning("Настройки внешней БД EnergoId не получены, актуализация пропускается");
			return;
		}
		EnvExpander.ExpandEnvVariables(settings);

		await using var connection = await dataSource.OpenConnectionAsync(ct);
		await using var transaction = await connection.BeginTransactionAsync(ct);

		try
		{
			// Создание расширения. Были проблемы, что схемы нет, так что схему создаем перед этим
			// Предварительно удаляем схему, чтобы не было конфликтов
			await Exec(connection,
				$"DROP SCHEMA IF EXISTS {QI(EnergoIdDefinitions.Schema)} CASCADE;", ct);

			await Exec(connection,
				$"CREATE SCHEMA {QI(EnergoIdDefinitions.Schema)};", ct);

			await Exec(connection,
				$"CREATE EXTENSION IF NOT EXISTS postgres_fdw SCHEMA {QI(EnergoIdDefinitions.Schema)};", ct);

			// Создание сервера
			await Exec(connection, $@"
				DO $$
				BEGIN
					IF NOT EXISTS (SELECT 1 FROM pg_foreign_server WHERE srvname = {QS(ExternalDbName)}) THEN
						CREATE SERVER {QI(ExternalDbName)}
							FOREIGN DATA WRAPPER postgres_fdw
							OPTIONS (host {QS(settings.Host)}, port {QS(settings.Port.ToString())}, dbname {QS(settings.Database)});
					ELSE
						ALTER SERVER {QI(ExternalDbName)}
							OPTIONS (SET host {QS(settings.Host)}, SET port {QS(settings.Port.ToString())}, SET dbname {QS(settings.Database)});
					END IF;
				END $$;", ct);

			// Создание/обновление пользователя для доступа
			await Exec(connection,
				$"CREATE USER MAPPING IF NOT EXISTS FOR {QI(settings.User)} SERVER {QI(ExternalDbName)} " +
				$"OPTIONS(user {QS(settings.User)}, password {QS(settings.Password)});", ct);

			await Exec(connection,
				$"ALTER USER MAPPING FOR {QI(settings.User)} SERVER {QI(ExternalDbName)} " +
				$"OPTIONS (SET user {QS(settings.User)}, SET password {QS(settings.Password)});", ct);

			// Пробуем обновить схему
			// Лучше конечно бы ее удалять и пересоздавать
			await Exec(connection,
				$"IMPORT FOREIGN SCHEMA {QI(settings.Schema)} LIMIT TO (realm, user_entity, user_attribute) FROM SERVER {QI(ExternalDbName)} INTO {QI(EnergoIdDefinitions.Schema)}", ct);

			await Exec(connection, @$"
				CREATE VIEW {QI(EnergoIdDefinitions.Schema)}.{QI(EnergoIdDefinitions.UsersView.ViewName)} AS
				WITH cte_user AS (
					SELECT ue.*
					FROM {QI(EnergoIdDefinitions.Schema)}.realm r
					INNER JOIN {QI(EnergoIdDefinitions.Schema)}.user_entity ue ON
						r.""name"" = 'energo' AND
						r.id = ue.realm_id 
				)
				SELECT
					(c.id)::uuid AS {QI(nameof(EnergoIdEntity.Guid))},
					c.username AS {QI(nameof(EnergoIdEntity.UserName))},
					c.email AS {QI(nameof(EnergoIdEntity.Email))},
					c.first_name AS {QI(nameof(EnergoIdEntity.FirstName))},
					c.last_name AS {QI(nameof(EnergoIdEntity.LastName))},
					c.enabled AS {QI(nameof(EnergoIdEntity.IsEnabled))},
					/* из миллисекунд → timestamptz (UTC) */
					(TIMESTAMP WITH TIME ZONE 'epoch' + (c.created_timestamp / 1000.0) * INTERVAL '1 second') AS {QI(nameof(EnergoIdEntity.CreatedAt))},
					ua.value  AS {QI(nameof(EnergoIdEntity.UploaderEnterpriseCode))},
					ua1.value AS {QI(nameof(EnergoIdEntity.EnterpriseCode))},
					ua2.value AS {QI(nameof(EnergoIdEntity.PersonnelNumber))},
					ua3.value AS {QI(nameof(EnergoIdEntity.MiddleName))},
					ua4.value AS {QI(nameof(EnergoIdEntity.Phone))},
					ua5.value AS {QI(nameof(EnergoIdEntity.WorkPhone))},
					ua6.value AS {QI(nameof(EnergoIdEntity.MobilePhone))},
					ua7.value AS {QI(nameof(EnergoIdEntity.Gender))},
					ua8.value AS {QI(nameof(EnergoIdEntity.Birthday))}
				FROM cte_user c
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua  ON c.id = ua.user_id  AND ua.""name""  = 'uploader_enterprise_code'
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua1 ON c.id = ua1.user_id AND ua1.""name"" = 'enterprise_code'
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua2 ON c.id = ua2.user_id AND ua2.""name"" = 'personnel_number'
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua3 ON c.id = ua3.user_id AND ua3.""name"" = 'middle_name'
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua4 ON c.id = ua4.user_id AND ua4.""name"" = 'phone'
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua5 ON c.id = ua5.user_id AND ua5.""name"" = 'work_phone'
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua6 ON c.id = ua6.user_id AND ua6.""name"" = 'mobile_phone'
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua7 ON c.id = ua7.user_id AND ua7.""name"" = 'gender'
				LEFT JOIN {QI(EnergoIdDefinitions.Schema)}.user_attribute ua8 ON c.id = ua8.user_id AND ua8.""name"" = 'birthday';", ct);

			await transaction.CommitAsync(ct);

			logger.LogInformation("Подключение к внешней БД EnergoId актуализировано");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Не удалось актуализировать подключение к внешней БД EnergoId");

			await transaction.RollbackAsync(ct);
		}
	}


	private const string ExternalDbName = "EnergoId";

	private static async Task Exec(NpgsqlConnection conn, string sql, CancellationToken ct)
	{
		await using var cmd = new NpgsqlCommand(sql, conn);

		//logger.LogDebug("SQL: {sql}", sql);
		await cmd.ExecuteNonQueryAsync(ct);
	}

	private static string QI(string ident) => "\"" + ident.Replace("\"", "\"\"") + "\"";

	private static string QS(string s) => "'" + s.Replace("'", "''") + "'";

	internal class ExternalDbOptions
	{
		public required string Host { get; set; }

		public required int Port { get; set; }

		public required string Database { get; set; }

		public required string Schema { get; set; }

		public required string User { get; set; }

		public required string Password { get; set; }
	}
}
