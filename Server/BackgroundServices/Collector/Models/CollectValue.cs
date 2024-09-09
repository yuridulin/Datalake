using Datalake.ApiClasses.Enums;

namespace Datalake.Server.BackgroundServices.Collector.Models;

/// <summary>
/// Информация о полученном значении
/// </summary>
public struct CollectValue
{
	/// <summary>
	/// Дата получения значения
	/// </summary>
	public DateTime DateTime { get; set; }

	/// <summary>
	/// Идентификатор тега, для которого предназначено это значение
	/// </summary>
	public Guid Guid { get; set; }

	/// <summary>
	/// Путь, по которому было получено значение
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// Достоверность значения
	/// </summary>
	public TagQuality Quality { get; set; }
}

/// <summary>
/// Компаратор сравнения двух значений с источника
/// </summary>
public class CollectValueComparer : IEqualityComparer<CollectValue>
{
	/// <summary>
	/// Проверка на одинаковость значений с источника
	/// </summary>
	/// <param name="x">Исходное значение</param>
	/// <param name="y">Сравниваемое значение</param>
	/// <returns>Являются ли одинаковыми</returns>
	public bool Equals(CollectValue x, CollectValue y)
	{
		return Equals(x.Value, y.Value) && x.Quality == y.Quality;
	}

	/// <summary>
	/// Получение хэша значения с источника
	/// </summary>
	/// <param name="obj">Значение</param>
	/// <returns>Хэш</returns>
	public int GetHashCode(CollectValue obj)
	{
		int hashValue = obj.Value?.GetHashCode() ?? 0;
		int hashQuality = obj.Quality.GetHashCode();
		return hashValue ^ hashQuality;
	}
}