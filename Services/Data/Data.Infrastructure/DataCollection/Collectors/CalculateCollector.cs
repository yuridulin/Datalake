using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Domain.Extensions;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using NCalc;
using static Datalake.Data.Application.Models.Tags.TagCalculationSettingsDto;

namespace Datalake.Data.Infrastructure.DataCollection.Collectors;

[Transient]
public class CalculateCollector(
	ISourcesActivityStore sourcesActivityStore,
	IValuesStore valuesStore,
	ITagsCollectionStatusStore errorsStore,
	IDataCollectorWriter writer,
	ILogger<DatalakeCollector> _,
	SourceSettingsDto source) : DataCollectorBase(sourcesActivityStore, writer, _, source)
{
	private readonly HashSet<int> tagsToRecalculate = [];
	private bool firstRun = true;
	private readonly Dictionary<int, HashSet<CalculationScope>> inputDependencies = [];

	public override Task StartAsync(CancellationToken cancellationToken)
	{
		if (!calculationScopes.Any())
			return NotStartAsync("нет правил расчета");

		BuildDependencyMap();
		valuesStore.ValuesChanged += ValuesStore_ValuesChanged;

		return base.StartAsync(cancellationToken);
	}

	private void ValuesStore_ValuesChanged(object? sender, ValuesChangedEventArgs e)
	{
		foreach (var changedTagId in e.ChangedTags)
		{
			if (inputDependencies.TryGetValue(changedTagId, out var dependentScopes))
			{
				foreach (var scope in dependentScopes)
				{
					lock (tagsToRecalculate)
					{
						tagsToRecalculate.Add(scope.TagId);
					}
				}
			}
		}
	}

	protected override async Task ExecuteAsync(CollectorUpdate state, CancellationToken cancellationToken)
	{
		// Получаем список выражений для пересчета
		HashSet<int> scopesToRecalculate;
		lock (tagsToRecalculate)
		{
			// При первом запуске пересчитываем все выражения
			if (firstRun)
			{
				scopesToRecalculate = calculationScopes.Select(s => s.TagId).ToHashSet();
				firstRun = false;
			}
			else
			{
				scopesToRecalculate = [.. tagsToRecalculate];
				tagsToRecalculate.Clear();
			}
		}

		if (scopesToRecalculate.Count == 0)
		{
			state.IsActive = true;
			return;
		}

		var now = DateTimeExtension.GetCurrentDateTime();

		foreach (var scope in calculationScopes.Where(s => scopesToRecalculate.Contains(s.TagId)))
		{
			var value = Calculate(scope, now);
			if (value != null)
				state.Values.Add(value);
		}

		state.IsActive = true;
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

	/// <summary>
	/// Строит карту зависимостей: для каждого входного тега определяет, какие выражения от него зависят
	/// </summary>
	private void BuildDependencyMap()
	{
		foreach (var scope in calculationScopes)
		{
			foreach (var input in scope.Inputs)
			{
				if (!inputDependencies.TryGetValue(input.SourceTagId, out var scopes))
				{
					scopes = [];
					inputDependencies[input.SourceTagId] = scopes;
				}
				scopes.Add(scope);
			}
		}
	}

	private TagValue? Calculate(CalculationScope scope, DateTime now)
	{
		string? error = null;

		// установка переменных для формулы
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
			return HandleError(scope.TagId, now, error);
		}

		// вычисление формулы
		object? result;
		try
		{
			result = scope.Expression.Evaluate();
		}
		catch (Exception ex)
		{
			return HandleError(scope.TagId, now, ex.Message
				.Replace("Parameter ", "Параметр [")
				.Replace(" not defined.", "] не найден"));
		}

		try
		{
			TagValue value = TagValue.FromRaw(scope.TagId, scope.TagType, now, TagQuality.Good, value: result, scope.TagScale);
			errorsStore.Set(scope.TagId, null);
			return value;
		}
		catch (Exception ex)
		{
			return HandleError(scope.TagId, now, ex.Message);
		}
	}

	private TagValue HandleError(int tagId, DateTime date, string error)
	{
		errorsStore.Set(tagId, error);

		return TagValue.AsEmpty(tagId, date, TagQuality.Bad_CalcError);
	}
}
