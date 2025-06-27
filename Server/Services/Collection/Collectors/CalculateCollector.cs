using Datalake.Database.InMemory;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Values;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;
using NCalc;

namespace Datalake.Server.Services.Collection.Collectors;

internal class CalculateCollector : CollectorBase
{
	public CalculateCollector(
		DatalakeCurrentValuesStore valuesStore,
		TagsStateService tagsStateService,
		SourceWithTagsInfo source,
		SourcesStateService sourcesStateService,
		ILogger<CalculateCollector> logger) : base("Расчетные значения", source, sourcesStateService, logger)
	{
		_valuesStore = valuesStore;
		_expressions = source.Tags
			.Select(tag =>
			{
				var expresssion = new Expression(tag.Formula);
				expresssion.EvaluateParameter += (name, args) => Expression_EvaluateParameter(name, args, tag, tagsStateService);

				return (tag, expresssion);
			})
			.ToArray();
	}

	public override void Start(CancellationToken stoppingToken)
	{
		if (_expressions.Length == 0)
		{
			_logger.LogWarning("Сборщик \"{name}\" не имеет правил расчета и не будет запущен", _name);
			return;
		}

		Task.Run(Work, stoppingToken);

		base.Start(stoppingToken);
	}


	private readonly DatalakeCurrentValuesStore _valuesStore;
	private readonly (SourceTagInfo tag, Expression expression)[] _expressions;

	private void Expression_EvaluateParameter(string name, NCalc.Handlers.ParameterArgs args, SourceTagInfo tag, TagsStateService tagsStateService)
	{
		var inputTag = tag.FormulaInputs.FirstOrDefault(x => x.VariableName == name);
		if (inputTag != null)
		{
			var value = _valuesStore.Get(inputTag.InputTagId);
			tagsStateService.UpdateTagState(inputTag.InputTagGuid, "calculate-collector");
			args.Result = value?.Number ?? 0;
		}
		else
		{
			args.Result = 0;
		}
	}

	protected override async Task Work()
	{
		List<ValueWriteRequest> batch = new();
		var now = DateFormats.GetCurrentDateTime();

		foreach (var (tag, expression) in _expressions)
		{
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
				var result = expression.Evaluate();

				if (tag.Type == TagType.Number)
				{
					record.Value = result == null ? null : Convert.ToSingle(result);
				}
				else if (tag.Type == TagType.String)
				{
					record.Value = result == null ? null : Convert.ToString(result);
				}
				else if (tag.Type == TagType.Boolean)
				{
					record.Value = result == null ? null : Convert.ToString(result);
				}

				record.Quality = TagQuality.Good;
			}
			catch (Exception ex)
			{
				record.Quality = TagQuality.Bad_NoValues;
				record.Value = ex.Message;
			}

			batch.Add(record);
		}

		await WriteAsync(batch);
	}
}
