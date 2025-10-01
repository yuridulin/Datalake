using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Exceptions;
using System.Diagnostics;

namespace Datalake.InventoryService.Application.Abstractions;

public abstract class TransactionalCommandHandler<TCommand, TResult>(
	IUnitOfWork unitOfWork,
	ILogger logger,
	IInventoryCache? inventoryCache = null) : ICommandHandler<TCommand, TResult>
		where TCommand : ICommandRequest
		where TResult : notnull
{
	private readonly string _commandName = typeof(TCommand).Name;

	protected readonly IUnitOfWork _unitOfWork = unitOfWork;
	protected readonly ILogger _logger = logger;

	public virtual void CheckPermissions(TCommand command) { }

	public abstract Task<TResult> ExecuteInTransactionAsync(TCommand command, CancellationToken ct = default);

	public virtual InventoryState UpdateCache(InventoryState state) => state;

	public virtual async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
	{
		using var activity = Activity.Current?.Source.StartActivity($"Handle {_commandName}");
		_logger.LogDebug("Выполнение команды {name}", _commandName);

		CheckPermissions(command);

		await _unitOfWork.BeginTransactionAsync(ct);

		TResult result;

		try
		{
			result = await ExecuteInTransactionAsync(command, ct);

			await _unitOfWork.SaveChangesAsync(ct);
			await _unitOfWork.CommitAsync(ct);
		}
		catch (Exception ex)
		{
			await _unitOfWork.RollbackAsync(ct);

			if (ex is InfrastructureException infrastructureException)
			{
				_logger.LogError(infrastructureException, "Инфраструктурная ошибка выполнения команды {name}", _commandName);
			}
			else if (ex is AppException appException)
			{
				_logger.Log(appException.LogLevel, "Команда {name} прекратила выполнение: {message}", _commandName, ex.Message);
			}
			else
			{
				_logger.LogError(ex, "Неизвестная ошибка при выполнении команды {name}", _commandName);
			}

			throw;
		}

		if (inventoryCache != null)
		{
			await inventoryCache.UpdateAsync(UpdateCache);
		}

		_logger.LogDebug("Выполнение команды {name} завершено", _commandName);
		return result;
	}
}
