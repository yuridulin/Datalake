using Microsoft.Extensions.Configuration;
using Npgsql;
using System.CommandLine;
using System.Text;

namespace Datalake.Backup.Data;

internal class Program
{
	static Encoding Encoding { get; set; } = null!;

	static async Task Main(string[] args)
	{
		var fromOption = new Option<DateTime?>(
			name: "--from",
			description: "Начало периода (yyyy-MM-dd)"
		);

		var toOption = new Option<DateTime?>(
			name: "--to",
			description: "Конец периода (yyyy-MM-dd)"
		);

		var dailyOption = new Option<bool>(
			name: "--daily",
			description: "Разбивать по дням",
			getDefaultValue: () => false
		);

		var rootCommand = new RootCommand
		{
			fromOption,
			toOption,
			dailyOption
		};

		rootCommand.SetHandler(async (from, to, daily) =>
		{
			// Здесь твоя логика
			Console.WriteLine($"from={from}, to={to}, daily={daily}");
			await BackupHistoryData(from, to, daily);
		},
		fromOption, toOption, dailyOption);

		await rootCommand.InvokeAsync(args);
	}

	static async Task BackupHistoryData(DateTime? from, DateTime? to = null, bool? daily = false)
	{
		// Устанавливаем UTF-8 для консоли
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		Encoding = Encoding.GetEncoding("windows-1251");

		Console.OutputEncoding = Encoding;
		Console.InputEncoding = Encoding;

		var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: false)
			.Build();

		var connectionString = new NpgsqlConnectionStringBuilder
			{
				Host = configuration["Database:Host"],
				Database = configuration["Database:Name"],
				Username = configuration["Database:User"],
				Password = configuration["Database:Password"],
				Port = int.Parse(configuration["Database:Port"]!)
			}
			.ToString();

		var outputDir = Path.Combine(Environment.CurrentDirectory, "data");

		Directory.CreateDirectory(outputDir);

		// Определяем период
		to ??= DateTime.UtcNow.Date.AddDays(1);
		from ??= to.Value.AddDays(-7);
		daily ??= false;

		if (daily ?? false)
		{
			for (var day = from.Value; day < to.Value; day = day.AddDays(1))
			{
				var nextDay = day.AddDays(1);
				var fileName = Path.Combine(outputDir, $"TagsHistory_{day:yyyy-MM-dd}.csv");
				await DumpTagsHistoryAsync(connectionString, day, nextDay, fileName);
				Console.WriteLine($"Saved {fileName}");
			}
		}
		else
		{
			var fileName = Path.Combine(outputDir, $"TagsHistory_{from:yyyy-MM-dd}_{to:yyyy-MM-dd}.csv");
			await DumpTagsHistoryAsync(connectionString, from.Value, to.Value, fileName);
			Console.WriteLine($"Saved {fileName}");
		}
	}

	static async Task DumpTagsHistoryAsync(string connString, DateTime from, DateTime to, string outputFile)
	{
		await using var conn = new NpgsqlConnection(connString);
		await conn.OpenAsync();

		var sql = $@"
			COPY (
				SELECT *
				FROM public.""TagsHistory""
				WHERE ""Date"" >= '{from:yyyy-MM-dd}' AND ""Date"" < '{to:yyyy-MM-dd}'
				ORDER BY ""Date""
			) TO STDOUT WITH (FORMAT csv, HEADER true)";

		await using var writer = new StreamWriter(outputFile, false, Encoding.UTF8);
		using var exporter = await conn.BeginTextExportAsync(sql);

		var buffer = new char[8192];
		int read;
		while ((read = await exporter.ReadAsync(buffer, 0, buffer.Length)) > 0)
		{
			await writer.WriteAsync(buffer, 0, read);
		}
	}
}
