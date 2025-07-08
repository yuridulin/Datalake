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

			expression.Parameters.Clear();

			string? error = null;

			foreach (var input in tag.FormulaInputs)
			{
				var inputRecord = valuesStore.Get(input.InputTagId);

				if (inputRecord == null)
				{
					error = $"Не найден входной тег #{input.InputTagId}";
					break;
				}

				usedTags.Add(inputRecord.TagId);

				object? inputValue = input.InputTagType switch
				{
					TagType.String => inputRecord.Text,
					TagType.Number => inputRecord.Number,
					TagType.Boolean => inputRecord.Number.HasValue ? inputRecord.Number == 1 : null,
				};

				if (inputValue == null)
				{
					error = $"У входного тега #{input.InputTagId} нет значения";
					break;
				}

				expression.Parameters[input.VariableName] = inputValue;
			}

			if (error == null)
			{
				try
				{
					var result = expression.Evaluate();

					if (result == null)
					{
						error = "Итоговое значение не получено";
					}
					else
					{
						record.Value = tag.Type switch
						{
							TagType.Number => Convert.ToSingle(result),
							TagType.String => Convert.ToString(result),
							TagType.Boolean => Convert.ToString(result),
						};
						record.Quality = TagQuality.Good;
					}
				}
				catch (Exception ex)
				{
					error = ex.Message;
				}
			}

			if (error != null)
			{
				record.Value = null;
				record.Quality = TagQuality.Bad_CalcError;

				_logger.LogDebug("CALC | #{tag}: {message}", tag.Id, error);
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
