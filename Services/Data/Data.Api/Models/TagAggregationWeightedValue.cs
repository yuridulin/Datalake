namespace Data.Api.Models;

/// <summary>
/// Результат вычисления взвешенного агрегатного значения
/// </summary>
public class TagAggregationWeightedValue
{
	public float SumOfWeights { get; set; }

	public float SumValuesWithWeights { get; set; }

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	public int TagId { get; set; }

	/// <summary>
	/// Актуальная дата
	/// </summary>
	public DateTime Date { get; set; }

	/// <summary>
	/// Взвешенное среднее
	/// </summary>
	public float? Average => SumOfWeights == 0 ? null : SumValuesWithWeights / SumOfWeights;

	/// <summary>
	/// Взвешенная сумма
	/// </summary>
	public float Sum => SumValuesWithWeights;
}
