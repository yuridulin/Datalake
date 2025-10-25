using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Datalake.Data.Infrastructure.DataCollection.Abstractions;

/// <summary>
/// Базовый класс сборщика с реализацией основных механизмов
/// </summary>
public abstract class DataCollectorBase(
	SourceSettingsDto sourceSettings,
	ILogger logger,
	int workInterval = 1000) : IDataCollector
{
	protected readonly SourceSettingsDto _source = sourceSettings;
	protected readonly ILogger _logger = logger;
	protected readonly CancellationTokenSource _tokenSource = new();
	protected readonly Channel<IEnumerable<TagValue>> _outputChannel = Channel.CreateUnbounded<IEnumerable<TagValue>>();
	protected readonly string _name = Source.InternalSources.Contains(sourceSettings.SourceType)
		? sourceSettings.SourceType.ToString()
		: $"{sourceSettings.SourceName}<{sourceSettings.SourceType}>#{sourceSettings.SourceId}";

	protected CancellationToken _stoppingToken;
	private volatile bool _stopped = false;

	public Channel<IEnumerable<TagValue>> OutputChannel => _outputChannel;

	public string Name => _name;

	public virtual void Start(CancellationToken stoppingToken = default)
	{
		_stopped = false;
		_stoppingToken = stoppingToken;
		_logger.LogDebug("Сборщик {name} запущен", _name);

		_ = WorkLoop();
	}

	protected virtual async Task WorkLoop()
	{
		while (!_tokenSource.Token.IsCancellationRequested)
		{
			try
			{
				await Work();
				await Task.Delay(workInterval, _tokenSource.Token);
			}
			catch (OperationCanceledException)
			{
				await WriteAsync([], false);
				break; // Выход при отмене
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				_logger.LogDebug("Сборщик {name}: {err}", _name, ex.Message);
				await WriteAsync([], false);
				await Task.Delay(5000, _tokenSource.Token);
			}
		}
		_logger.LogDebug("Сборщик {name} завершил работу", _name);
	}

	protected abstract Task Work();

	public virtual void PrepareToStop()
	{
		_tokenSource.Cancel();
		_outputChannel.Writer.TryComplete();
		_logger.LogDebug("Сборщик {name} готовится к остановке", _name);
	}

	public virtual void Stop()
	{
		if (_stopped)
			return;
		_stopped = true;

		_logger.LogDebug("Сборщик {name} окончательно остановлен", _name);
	}

	protected virtual async Task WriteAsync(IEnumerable<TagValue> values, bool connected = true)
	{
		if (_stopped || _tokenSource.IsCancellationRequested)
			return;

		if (!values.Any())
			return;

		try
		{
			await _outputChannel.Writer.WriteAsync(values, _stoppingToken);
		}
		catch (ChannelClosedException)
		{
		}
	}
}
