namespace Datalake.Data.Infrastructure.DataCollection.Models;

/// <summary>
/// Результат вычисления взвешенного агрегатного значения
/// </summary>
public record TagWeightedValue
{
	public TagWeightedValue(int tagId, DateTime date, float weightsSum, float valuesSum)
	{
		TagId = tagId;
		Date = date;
		_weightsSum = weightsSum;
		_valuesSum = valuesSum;
	}

	private float _weightsSum;
	private float _valuesSum;

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	public int TagId { get; init; }

	/// <summary>
	/// Актуальная дата
	/// </summary>
	public DateTime Date { get; init; }

	/// <summary>
	/// Взвешенное среднее
	/// </summary>
	public float? Average => _weightsSum == 0 ? null : _valuesSum / _weightsSum;

	/// <summary>
	/// Взвешенная сумма
	/// </summary>
	public float Sum => _valuesSum;
}
