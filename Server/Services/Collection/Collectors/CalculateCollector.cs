using Datalake.Database.InMemory;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;
using NCalc;

namespace Datalake.Server.Services.Collection.Collectors;

internal class CalculateCollector(
	DatalakeCurrentValuesStore valuesStore,
	TagsStateService tagsStateService,
	SourceWithTagsInfo source,
	SourcesStateService sourcesStateService,
	ILogger<CalculateCollector> logger) : CollectorBase("Расчетные значения", source, sourcesStateService, logger)
{
	public override void Start(CancellationToken stoppingToken)
	{
		if (_expressions.Length == 0)
		{
			_logger.LogWarning("Сборщик \"{name}\" не имеет правил расчета и не будет запущен", _name);
			return;
		}

		base.Start(stoppingToken);
	}

	public override void PrepareToStop()
	{
		base.PrepareToStop();

		_expressions = [];
	}

	protected override async Task Work()
	{
		var now = DateFormats.GetCurrentDateTime();
		List<ValueWriteRequest> batch = [];
		HashSet<int> usedTags = [];

		foreach (var (tag, expression) in _expressions)
		{
			if (_tokenSource.IsCancellationRequested)
				break;

			var record = new ValueWriteRequest
			{
				Date = now,
				Guid = tag.Guid,
				Id = tag.Id,
				Name = tag.Name,
				Value = null,
				Quality = TagQuality.Bad_NoConnect,
			};

			try
			{
				expression.Parameters.Clear();
				foreach (var input in tag.FormulaInputs)
				{
					var inputValue = valuesStore.Get(input.InputTagId);

					if (inputValue != null)
					{
						if (input.InputTagType == TagType.String)
							expression.Parameters[input.VariableName] = inputValue.Text ?? string.Empty;
						else if (input.InputTagType == TagType.Boolean)
							expression.Parameters[input.VariableName] = inputValue.Number == 1;
						else
							expression.Parameters[input.VariableName] = inputValue.Number ?? 0;

						usedTags.Add(inputValue.TagId);
					}
					else
					{
						expression.Parameters[input.VariableName] = 0;
					}
				}

				var result = expression.Evaluate();

				if (result != null)
				{
					record.Value = result;
				}
				else
				{
					record.Value = tag.Type switch
					{
						TagType.Number => Convert.ToSingle(result),
						TagType.String => Convert.ToString(result),
						TagType.Boolean => Convert.ToString(result),
					};
				}

				record.Quality = TagQuality.Good;
			}
			catch (Exception ex)
			{
				record.Quality = TagQuality.Bad_CalcError;

				_logger.LogDebug("CALC | #{tag}: {message}." +
					"\nFormula [{formula}]" +
					"\nType {type}" +
					"\nResult {result}",
					tag.Id, ex.Message, expression.ExpressionString, tag.Type, record.Value);
			}

			batch.Add(record);
		}

		await WriteAsync(batch);

		foreach (var tagId in usedTags)
			tagsStateService.UpdateTagState(tagId, CollectorRequestKey);
	}

	const string CollectorRequestKey = "calculate-collector";
	private (SourceTagInfo tag, Expression expression)[] _expressions = source.Tags
		.Select(tag => (tag, new Expression(tag.Formula)))
		.ToArray();
}
