using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Domain.Exceptions;
using System.Globalization;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице истории значений тегов
/// </summary>
public sealed record class TagValue
{
	#region Конструкторы

	private TagValue() { }

	/// <summary>
	/// Пустое значение, подходит для любого типа данных
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="date">Дата</param>
	/// <param name="quality">Качество</param>
	/// <returns>Значение тега</returns>
	public static TagValue AsEmpty(int tagId, DateTime? date, TagQuality? quality)
	{
		return new()
		{
			TagId = tagId,
			Date = date ?? DateTimeExtension.GetCurrentDateTime(),
			Quality = quality ?? TagQuality.Bad_NoValues,
		};
	}

	/// <summary>
	/// Типизированное значение из сырого исходного значения
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="type">Тип данных тега</param>
	/// <param name="date">Дата</param>
	/// <param name="quality">Качество</param>
	/// <param name="value">Исходное значение</param>
	/// <param name="scale">Шкала преобразования для числового типа</param>
	/// <returns>Значение тега</returns>
	/// <exception cref="DomainException">Тип не поддерживается</exception>
	public static TagValue FromRaw(int tagId, TagType type, DateTime? date, TagQuality? quality, object? value, float? scale)
	{
		if (value == null)
			return AsEmpty(tagId, date, quality);

		var text = value.ToString();

		return type switch
		{
			TagType.String => AsString(tagId, date, quality, text),

			TagType.Number => double.TryParse(text ?? "x", NumberStyles.Float, CultureInfo.InvariantCulture, out double dValue)
				? AsNumeric(tagId, date, quality, (float?)dValue, scale) // TODO: выглядит опасно, нужен тест
				: AsEmpty(tagId, date, quality),

			TagType.Boolean => AsBoolean(tagId, date, quality, text == One || text == True),

			_ => throw new DomainException("Неизвестный тип при создании значения: " + type.ToString())
		};
	}

	/// <summary>
	/// Типизированное значение из исходного строкового значения
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="date">Дата</param>
	/// <param name="quality">Качество</param>
	/// <param name="text">Исходное строковое значение</param>
	/// <returns>Значение тега</returns>
	public static TagValue AsString(int tagId, DateTime? date, TagQuality? quality, string? text)
	{
		var value = AsEmpty(tagId, date, quality);
		value.Text = text;
		return value;
	}

	/// <summary>
	/// Типизированное значение из исходного числового значения
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="date">Дата</param>
	/// <param name="quality">Качество</param>
	/// <param name="number">Исходное числовое значение</param>
	/// <param name="scale">Шкала преобразования для числового типа</param>
	/// <returns>Значение тега</returns>
	public static TagValue AsNumeric(int tagId, DateTime? date, TagQuality? quality, float? number, float? scale)
	{
		var value = AsEmpty(tagId, date, quality);
		value.Number = scale.HasValue ? (number * scale) : number;
		return value;
	}

	/// <summary>
	/// Типизированное значение из исходного логического значения
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="date">Дата</param>
	/// <param name="quality">Качество</param>
	/// <param name="boolean">Исходное логическое значение</param>
	/// <returns>Значение тега</returns>
	public static TagValue AsBoolean(int tagId, DateTime? date, TagQuality? quality, bool? boolean)
	{
		var value = AsEmpty(tagId, date, quality);
		value.Boolean = boolean;
		return value;
	}

	#endregion Конструкторы

	#region Свойства

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	public int TagId { get; private set; }

	/// <summary>
	/// Дата
	/// </summary>
	public DateTime Date { get; private set; }

	/// <summary>
	/// Текстовое значение
	/// </summary>
	public string? Text { get; private set; } = null;

	/// <summary>
	/// Числовое значение
	/// </summary>
	public float? Number { get; private set; } = null;

	/// <summary>
	/// Логическое значение
	/// </summary>
	public bool? Boolean { get; private set; } = null;

	/// <summary>
	/// Флаг качества
	/// </summary>
	public TagQuality Quality { get; private set; }

	#endregion Свойства

	#region Методы

	/// <summary>
	/// Проверка, является ли входящее значение новым относительно текущего.
	/// Новым будет считаться, если не старее по времени и содержит новые данные
	/// </summary>
	public bool IsNew(DateTime date, string? text = null, float? number = null, bool? boolean = null)
	{
		if (date < Date)
			return false; // запись в прошлое

		return text != Text || boolean != Boolean || !AreAlmostEqual(number, Number);
	}

	/// <summary>
	/// Проверка, является ли входящее значение новым относительно текущего.
	/// Новым будет считаться, если не старее по времени и содержит новые данные
	/// </summary>
	public bool IsNew(TagValue newValue)
	{
		return IsNew(newValue.Date, newValue.Text, newValue.Number, newValue.Boolean);
	}

	/// <summary>
	/// Протягивание значения вперед на указанную дату
	/// </summary>
	/// <param name="date">Дата</param>
	/// <returns>Значение на новую дату</returns>
	/// <exception cref="DomainException">Дата не подходит</exception>
	public TagValue StretchToDate(DateTime date)
	{
		if (Date > date)
			throw new DomainException("Переданное значение старше, чем текущее. Нельзя протянуть значение тега в прошлое", nameof(date));

		if (Date == date)
			return this;

		return new TagValue
		{
			TagId = TagId,
			Date = date,
			Text = Text,
			Number = Number,
			Boolean = Boolean,
			// протягивание качества
			Quality = (byte)Quality < (byte)TagQuality.Good
				? TagQuality.Bad_LOCF
				: TagQuality.Good_LOCF,
		};
	}

	private static bool AreAlmostEqual(float? value1, float? value2, double epsilon = 0.00001)
	{
		var rounded = Math.Abs((value1 ?? 0) - (value2 ?? 0));
		return rounded < epsilon;
	}

	#endregion Методы

	#region Константы

	/// <summary>
	/// Значение истины
	/// </summary>
	private static string True { get; } = true.ToString();

	/// <summary>
	/// Значение единицы
	/// </summary>
	private static string One { get; } = 1.ToString();

	#endregion
}
