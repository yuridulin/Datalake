using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using NCalc;
using static Datalake.Data.Application.Models.Tags.TagCalculationSettingsDto;

namespace Datalake.Data.Infrastructure.DataCollection.DataCollectors;

[Transient]
public class CalculateCollector(
	ICurrentValuesStore valuesStore,
	IDataCollectionErrorsStore errorsStore,
	SourceSettingsDto source,
	ILogger<DatalakeCollector> logger) : DataCollectorBase(source, logger)
{
	public override void Start(CancellationToken stoppingToken)
	{
		// если тегов нет, то и работать незачем
		if (!calculationScopes.Any())
		{
			Task.Run(() => WriteAsync([], false), stoppingToken);
			_logger.LogWarning("Сборщик \"{name}\" не имеет правил расчета и не будет запущен", _name);
			return;
		}

		Task.Run(() => WriteAsync([], true), stoppingToken);
		base.Start(stoppingToken);
	}

	public override void PrepareToStop()
	{
		base.PrepareToStop();

		calculationScopes = [];
	}

	protected override async Task Work()
	{
		var now = DateTimeExtension.GetCurrentDateTime();
		List<TagHistoryValue> batch = [];

		foreach (var scope in calculationScopes)
		{
			if (_tokenSource.IsCancellationRequested)
				break;

			// установка переменных для формулы
			string? error = null;
			scope.Expression.Parameters.Clear();
			foreach (var input in scope.Inputs)
			{
				var inputRecord = valuesStore.TryGet(input.SourceTagId);
				if (inputRecord == null)
				{
					error = $"Тег переменной {input.VariableName} не найден";
					break;
				}

				switch (input.SourceTagType)
				{
					case TagType.String:
						scope.Expression.Parameters[input.VariableName] = inputRecord.Text;
						break;

					case TagType.Number:
						scope.Expression.Parameters[input.VariableName] = inputRecord.Number;
						break;

					case TagType.Boolean:
						scope.Expression.Parameters[input.VariableName] = inputRecord.Boolean;
						break;

					default:
						error = $"Тег переменной {input.VariableName} имеет неподдерживаемый тип данных";
						break;
				}

				if (error != null)
					break;
			}

			if (error != null)
			{
				batch.Add(HandleError(scope.TagId, now, error));
				continue;
			}

			// вычисление формулы
			object? result;
			try
			{
				result = scope.Expression.Evaluate();
			}
			catch (Exception ex)
			{
				batch.Add(HandleError(scope.TagId, now, ex.Message
					.Replace("Parameter ", "Параметр [")
					.Replace(" not defined.", "] не найден")));
				continue;
			}

			try
			{
				TagHistoryValue value = TagHistoryValue.FromRaw(scope.TagId, scope.TagType, now, TagQuality.Good, value: result, scope.TagScale);
				errorsStore.Set(scope.TagId, null);
				batch.Add(value);
			}
			catch (Exception ex)
			{
				batch.Add(HandleError(scope.TagId, now, ex.Message));
			}
		}

		await WriteAsync(batch);
	}

	/// <summary>
	/// Формулы вычисления
	/// </summary>
	private IEnumerable<CalculationScope> calculationScopes = source.NotDeletedTags
		.Where(tag => tag.CalculationSettings != null)
		.Select(tag => new CalculationScope
		{
			TagId = tag.TagId,
			TagType = tag.TagType,
			TagScale = tag.ScaleSettings?.GetScale() ?? null,
			Expression = new Expression(tag.CalculationSettings!.ExpressionFormula, ExpressionOptions.AllowNullParameter),
			Inputs = tag.CalculationSettings.ExpressionVariables,
		})
		.ToArray();

	private record class CalculationScope
	{
		public required Expression Expression { get; init; }

		public required int TagId { get; init; }

		public required TagType TagType { get; init; }

		public required float? TagScale { get; init; }

		public required IEnumerable<TagCalculationInputDto> Inputs { get; init; } = [];
	}

	private TagHistoryValue HandleError(int tagId, DateTime date, string error)
	{
		errorsStore.Set(tagId, error);

		return TagHistoryValue.AsEmpty(tagId, date, TagQuality.Bad_CalcError);
	}
}
