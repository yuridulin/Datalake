using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using System.Threading.Channels;

namespace Datalake.Server.Services.Collection.Abstractions;

/// <summary>
/// Базовый класс сборщика с реализацией основных механизмов
/// </summary>
/// <param name="name">Название источника данных</param>
/// <param name="source">Данные источника данных, необходимые для запуска сбора</param>
/// <param name="logger">Служба сообщений</param>
internal abstract class CollectorBase(
	string name,
	SourceWithTagsInfo source,
	ILogger logger) : ICollector
{
	protected readonly CancellationTokenSource tokenSource = new();
	protected readonly string _name = $"{name} #{source.Id}";
	protected readonly ILogger _logger = logger;
	protected readonly SourceType _sourceType = source.Type;
	protected readonly Channel<IEnumerable<ValueWriteRequest>> _outputChannel = Channel.CreateUnbounded<IEnumerable<ValueWriteRequest>>();
	protected CancellationToken _stoppingToken;
	private volatile bool _stopped = false;

	public Channel<IEnumerable<ValueWriteRequest>> OutputChannel => _outputChannel;

	public string Name => _name;

	public virtual void Start(CancellationToken stoppingToken)
	{
		_stopped = false;
		_stoppingToken = stoppingToken;
		_logger.LogDebug("Сборщик {name} запущен", _name);
	}

	/*public virtual void Stop()
	{
		if (_stopped)
			return;
		_stopped = true;

		tokenSource.Cancel();
		_outputChannel.Writer.Complete();

		_logger.LogDebug("Сборщик {name} остановлен", _name);
	}*/

	public virtual void PrepareToStop()
	{
		// Устанавливаем флаг остановки
		tokenSource.Cancel();
		_logger.LogDebug("Сборщик {name} готовится к остановке", _name);
	}

	public virtual void FinalStop()
	{
		if (_stopped)
			return;
		_stopped = true;

		_outputChannel.Writer.Complete();
		_logger.LogDebug("Сборщик {name} окончательно остановлен", _name);
	}

	protected virtual async Task WriteAsync(IEnumerable<ValueWriteRequest> values)
	{
		if (_stopped || tokenSource.IsCancellationRequested || !values.Any())
			return;

		try
		{
			await _outputChannel.Writer.WriteAsync(values, _stoppingToken);
		}
		catch (ChannelClosedException)
		{
			// Корректно игнорируем после остановки
		}
	}
}
