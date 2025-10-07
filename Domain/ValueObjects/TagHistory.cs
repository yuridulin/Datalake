using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Domain.Exceptions;
using System.Globalization;

namespace Datalake.Domain.ValueObjects;

/// <summary>
/// Запись в таблице истории значений тегов
/// </summary>
public sealed record class TagHistory
{
	private TagHistory() { }

	public TagHistory(int tagId, DateTime? date, TagQuality? quality)
	{
		TagId = tagId;
		Date = date ?? DateTimeExtension.GetCurrentDateTime();
		Quality = quality ?? TagQuality.Bad_NoValues;
	}

	public TagHistory(int tagId, TagType type, DateTime? date, TagQuality? quality, object? value, float? scale) : this(tagId, date, quality)
	{
		if (value == null)
			return;

		var text = value.ToString();

		if (type == TagType.String)
		{
			Text = text;
		}
		else if (type == TagType.Number)
		{
			if (double.TryParse(text ?? "x", NumberStyles.Float, CultureInfo.InvariantCulture, out double dValue))
			{
				float number = (float)dValue;

				if (scale.HasValue)
				{
					Number = number * scale;
				}
				else
				{
					Number = number;
				}
			}
		}
		else if (type == TagType.Boolean)
		{
			Boolean = text == One || text == True;
		}
		else
		{
			throw new DomainException("Неизвестный тип при создании значения: " + type.ToString());
		}
	}

	public TagHistory(int tagId, DateTime? date, TagQuality? quality, string? text) : this(tagId, date, quality)
	{
		Text = text;
	}

	public TagHistory(int tagId, DateTime? date, TagQuality? quality, float? number, float? scale) : this(tagId, date, quality)
	{
		if (scale.HasValue)
		{
			Number = number * scale;
		}
		else
		{
			Number = number;
		}
	}

	public TagHistory(int tagId, DateTime? date, TagQuality? quality, bool? boolean) : this(tagId, date, quality)
	{
		Boolean = boolean;
	}

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
	public bool IsNew(TagHistory newValue)
	{
		return IsNew(newValue.Date, newValue.Text, newValue.Number, newValue.Boolean);
	}

	/// <summary>
	/// Протягивание значения вперед на указанную дату
	/// </summary>
	/// <param name="date">Дата</param>
	/// <returns>Значение на новую дату</returns>
	/// <exception cref="DomainException">Дата не подходит</exception>
	public TagHistory StretchToDate(DateTime date)
	{
		if (Date > date)
			throw new DomainException("Переданное значение старше, чем текущее. Нельзя протянуть значение тега в прошлое", nameof(date));

		if (Date == date)
			return this;

		return new TagHistory
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

	/// <summary>
	/// Значение истины
	/// </summary>
	private static string True { get; } = true.ToString();

	/// <summary>
	/// Значение единицы
	/// </summary>
	private static string One { get; } = 1.ToString();

}
