using Datalake.Database.InMemory;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using Datalake.Server.Services.StateManager;
using NCalc;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class CalculateCollector : CollectorBase
{
	public CalculateCollector(
		DatalakeCurrentValuesStore valuesStore,
		SourceWithTagsInfo source,
		TagsStateService tagsStateService,
		ILogger<CalculateCollector> logger) : base("Расчетные значения", source, logger)
	{
		_inputs = [];
		_expressions = [];
		_tokenSource = new CancellationTokenSource();
		_valuesStore = valuesStore;

		foreach (var tag in source.Tags)
		{
			var scopedTag = new TagExpressionScope
			{
				Guid = tag.Guid,
				Name = tag.Name,
				Type = tag.Type,
				Expression = new Expression(tag.Formula)
			};

			scopedTag.Expression.EvaluateParameter += (name, args) => Expression_EvaluateParameter(name, args, tag, tagsStateService);

			var initial = _valuesStore.Get(tag.Id);

			if (scopedTag.Type == TagType.Number)
				scopedTag.PreviousNumber = initial?.Number;
			else
				scopedTag.PreviousValue = initial?.Text;

			_expressions.Add(tag.Id, scopedTag);

			foreach (var input in tag.FormulaInputs)
				_inputs.Add(input.InputTagId);
		}
	}


	public override event CollectEvent? CollectValues;

	public override Task Start(CancellationToken stoppingToken)
	{
		if (_expressions.Count == 0)
			return Task.CompletedTask;

		Task.Run(Work, stoppingToken);

		return base.Start(stoppingToken);
	}

	public override Task Stop()
	{
		_tokenSource.Cancel();

		return base.Stop();
	}

	private readonly CancellationTokenSource _tokenSource;
	private readonly DatalakeCurrentValuesStore _valuesStore;
	private readonly Dictionary<int, TagExpressionScope> _expressions;
	private readonly HashSet<int> _inputs;

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

	private async Task Work()
	{
		_logger.LogDebug("Старт вычислителя");

		while (!_tokenSource.Token.IsCancellationRequested)
		{
			try
			{
				List<CollectValue> batch = new();
				var now = DateFormats.GetCurrentDateTime();
				var countGood = 0;

				foreach (var (tagId, tagScope) in _expressions)
				{
					bool isCalculatedCorrectly = true;
					object? result = null;
					try
					{
						result = tagScope.Expression.Evaluate();
						countGood++;
					}
					catch
					{
						isCalculatedCorrectly = false;
					}

					switch (tagScope.Type)
					{
						case TagType.Number:
							float value = Convert.ToSingle(result);
							if (AreAlmostEqual(value, tagScope.PreviousNumber))
								continue;
							tagScope.PreviousNumber = value;
							break;

						default:
							if (Equals(result, tagScope.PreviousValue))
								continue;
							tagScope.PreviousValue = result;
							break;
					}

					batch.Add(new CollectValue
					{
						Date = now,
						Id = tagId,
						Guid = tagScope.Guid,
						Name = tagScope.Name,
						Quality = isCalculatedCorrectly ? TagQuality.Good : TagQuality.Bad,
						Value = result,
					});
				}

				CollectValues?.Invoke(this, batch);
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Ошибка в вычислителе: {message}", ex.Message);
			}
			finally
			{
				await Task.Delay(1000);
			}
		}
	}

	private static bool AreAlmostEqual(float? value1, float? value2, double epsilon = 0.00001)
	{
		var rounded = Math.Abs((value1 ?? 0) - (value2 ?? 0));
		return rounded < epsilon;
	}

	private class TagExpressionScope
	{
		public required Guid Guid;
		public required string Name;
		public required TagType Type;
		public required Expression Expression;
		public float? PreviousNumber;
		public object? PreviousValue;
	}
}
