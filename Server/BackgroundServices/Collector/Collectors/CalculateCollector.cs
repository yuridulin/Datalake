using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Models.Sources;
using Datalake.Database.Repositories;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using NCalc;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

internal class CalculateCollector : CollectorBase
{
	public CalculateCollector(
	SourceWithTagsInfo source,
	ILogger<CalculateCollector> logger) : base(source, logger)
	{
		_logger = logger;
		_expressions = [];
		_previousValues = [];
		_tokenSource = new CancellationTokenSource();

		foreach (var tag in source.Tags)
		{
			var expression = new Expression(tag.Formula);
			expression.EvaluateParameter += (name, args) => Expression_EvaluateParameter(name, args, tag);

			_expressions.Add((tag, expression));
			_previousValues.Add(tag.Id, ValuesRepository.GetLiveValue(tag.Id));
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

	private ILogger<CalculateCollector> _logger;
	private readonly CancellationTokenSource _tokenSource;
	private readonly Dictionary<int, object?> _previousValues;
	private readonly List<(SourceTagInfo, Expression)> _expressions;

	private static void Expression_EvaluateParameter(string name, NCalc.Handlers.ParameterArgs args, SourceTagInfo tag)
	{
		var inputTag = tag.FormulaInputs.FirstOrDefault(x => x.VariableName == name);
		if (inputTag != null)
		{
			var value = ValuesRepository.GetLiveValue(inputTag.InputTagId);
			args.Result = value ?? 0;
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

				foreach (var (tag, expression) in _expressions)
				{
					bool isCalculatedCorrectly = true;
					object? result = null;
					try
					{
						result = expression.Evaluate();
						countGood++;
					}
					catch
					{
						isCalculatedCorrectly = false;
					}

					switch (tag.Type)
					{
						case TagType.Number:
							var previous = (float?)_previousValues[tag.Id];
							float value = Convert.ToSingle(result);
							if (AreAlmostEqual(value, previous))
								continue;
							_previousValues[tag.Id] = value;
							break;

						default:
							if (Equals(_previousValues[tag.Id], result))
								continue;
							_previousValues[tag.Id] = result;
							break;
					}

					batch.Add(new CollectValue
					{
						Date = now,
						Guid = tag.Guid,
						Id = tag.Id,
						Name = tag.Name,
						Quality = isCalculatedCorrectly ? TagQuality.Good : TagQuality.Bad,
						Value = result,
					});
				}
#if DEBUG
				_logger.LogInformation("Вычислено: {all} (успешно {good}), новых: {new}", _expressions.Count, countGood, batch.Count);
#endif
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
}

