using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Maintenance;
using System.Threading.Channels;

namespace Datalake.Server.Services.Collection.Abstractions;

/// <summary>
/// Базовый класс сборщика с реализацией основных механизмов
/// </summary>
internal abstract class CollectorBase : ICollector
{
	public CollectorBase(
		string name,
		SourceWithTagsInfo source,
		SourcesStateService sourcesStateService,
		ILogger logger,
		int workInterval = 1000)
	{
		_source = source;
		_name = source.Id > 0 ? $"{name}<{source.Type}>#{source.Id}" : source.Type.ToString();
		_logger = logger;
		_stateService = sourcesStateService;
		_workInterval = workInterval;

		_stateService.UpdateSource(source.Id, false);
	}

	protected readonly SourcesStateService _stateService;
	protected readonly SourceWithTagsInfo _source;
	protected readonly string _name;
	protected readonly ILogger _logger;
	private readonly int _workInterval;

	protected readonly CancellationTokenSource _tokenSource = new();
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

		_stateService.UpdateSource(_source.Id, false);

		_ = WorkLoop();
	}

	protected virtual async Task WorkLoop()
	{
		while (!_tokenSource.Token.IsCancellationRequested)
		{
			try
			{
				await Work();
				await Task.Delay(_workInterval, _tokenSource.Token);
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

	public virtual void FinalStop()
	{
		if (_stopped)
			return;
		_stopped = true;

		_stateService.UpdateSource(_source.Id, false);
		_logger.LogDebug("Сборщик {name} окончательно остановлен", _name);
	}

	protected virtual async Task WriteAsync(IEnumerable<ValueWriteRequest> values, bool connected = true)
	{
		if (_stopped || _tokenSource.IsCancellationRequested)
			return;

		_stateService.UpdateSource(_source.Id, connected);

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
