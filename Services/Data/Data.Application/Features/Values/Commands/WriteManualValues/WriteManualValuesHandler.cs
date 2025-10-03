using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.Values.Commands.WriteManualValues;

public interface IWriteManualValuesHandler : ICommandHandler<WriteManualValuesCommand, WriteManualValuesResult> { }

public class WriteManualValuesHandler : IWriteManualValuesHandler
{
	public Task<WriteManualValuesResult> HandleAsync(WriteManualValuesCommand command, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
