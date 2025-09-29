using Datalake.InventoryService.Application.Interfaces;
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

	public virtual void CheckPermissions(TCommand command) { }

	public abstract Task<TResult> ExecuteInTransactionAsync(TCommand command, CancellationToken ct = default);

	public virtual Func<InventoryState, InventoryState>? UpdateCache { get; } = null;

	public virtual async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
	{
		using var activity = Activity.Current?.Source.StartActivity($"Handle {_commandName}");
		logger.LogDebug("Выполнение команды {name}", _commandName);

		CheckPermissions(command);

		await unitOfWork.BeginTransactionAsync(ct);

		TResult result;

		try
		{
			result = await ExecuteInTransactionAsync(command, ct);

			await unitOfWork.SaveChangesAsync(ct);
			await unitOfWork.CommitAsync(ct);
		}
		catch (Exception ex)
		{
			await unitOfWork.RollbackAsync(ct);

			if (ex is InfrastructureException infrastructureException)
			{
				logger.LogError(infrastructureException, "Инфраструктурная ошибка выполнения команды {name}", _commandName);
			}
			else if (ex is AppException appException)
			{
				logger.Log(appException.LogLevel, "Команда {name} прекратила выполнение: {message}", _commandName, ex.Message);
			}
			else
			{
				logger.LogError(ex, "Неизвестная ошибка при выполнении команды {name}", _commandName);
			}

			throw;
		}

		if (inventoryCache != null && UpdateCache != null)
			await inventoryCache.UpdateAsync(UpdateCache);

		logger.LogDebug("Выполнение команды {name} завершено", _commandName);
		return result;
	}
}
