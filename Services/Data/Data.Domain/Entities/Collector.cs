using Datalake.Contracts.Public.Enums;

namespace Datalake.Data.Domain.Entities;

public class Collector
{
	private Collector() { }

	public Collector(int id, SourceType type, string? host, int? port)
	{
		Id = id;
		Type = type;
		RemoteOptions = new(host, port);
	}

	public int Id { get; set; }

	public SourceType Type { get; set; }

	public RemoteOptions? RemoteOptions { get; set; }
}

public class RemoteOptions
{
	private RemoteOptions() { }

	public RemoteOptions(string? host, int? port)
	{
		ArgumentNullException.ThrowIfNull(host);
		if (!port.HasValue)
		{
			throw new ArgumentNullException(nameof(port));
		}

		Host = host;
		Port = port.Value;
	}

	public string Host { get; init; } = string.Empty;

	public int Port { get; init; }
}
