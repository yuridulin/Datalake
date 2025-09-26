using Datalake.PublicApi.Enums;

namespace Datalake.DataService.Database.Entities;

/// <summary>
/// Запись в таблице истории значений тегов
/// </summary>
public record class TagHistory
{
	private TagHistory() { }

	/// <summary>
	/// Создание значения тега
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="date">Дата</param>
	public TagHistory(
		int tagId,
		DateTime date,
		string? text,
		float? number,
		TagQuality quality)
	{
		TagId = tagId;
		Date = date;
		Text = text;
		Number = number;
		Quality = quality;
	}

	/// <summary>
	/// Создание несуществующего значения тега
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="date">Дата</param>
	/// <param name="customQuality">Качество. Можно переопределить, если причина отсутствия значения известна</param>
	public TagHistory(
		int tagId,
		DateTime date,
		TagQuality customQuality = TagQuality.Bad_NoValues)
	{
		TagId = tagId;
		Date = date;
		Text = null;
		Number = null;
		Quality = customQuality;
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
	/// Флаг качества
	/// </summary>
	public TagQuality Quality { get; private set; } = TagQuality.Good;


	/// <summary>
	/// Получение значения в зависимости от требуемого типа данных
	/// </summary>
	/// <param name="type">Требуемый тип данных</param>
	/// <returns>Значение в унифицированном виде</returns>
	public object? GetTypedValue(TagType type) => type switch
	{
		TagType.String => Text,
		TagType.Number => Number,
		TagType.Boolean => Number.HasValue ? Number != 0 : null,
		_ => null,
	};

	/// <summary>
	/// Проверка, является ли входящее значение новым (содержит новые данные или имеет время позднее)
	/// </summary>
	/// <param name="incomingValue"></param>
	/// <returns></returns>
	public bool IsNew(TagHistory incomingValue)
	{
		if (incomingValue.Date < Date)
			return false; // запись в прошлое
		else
		{
			if (!AreAlmostEqual(incomingValue.Number, Number) || incomingValue.Text != Text || incomingValue.Quality != Quality)
				return true; // значения не совпадают

			return false; // значения совпали, значит повтор
		}

		static bool AreAlmostEqual(float? value1, float? value2, double epsilon = 0.00001)
		{
			var rounded = Math.Abs((value1 ?? 0) - (value2 ?? 0));
			return rounded < epsilon;
		}
	}

	/// <summary>
	/// Протягивание значения вперед на указанную дату
	/// </summary>
	/// <param name="date">Дата</param>
	/// <returns>Значение на новую дату</returns>
	/// <exception cref="ArgumentException">Дата не подходит</exception>
	public TagHistory StretchToDate(DateTime date)
	{
		if (Date > date)
			throw new ArgumentException("Переданное значение старше, чем текущее. Нельзя протянуть значение тега в прошлое", nameof(date));

		if (Date == date)
			return this;

		return new(
			TagId,
			date,
			Text,
			Number,
			// протягивание качества
			(int)Quality < (int)TagQuality.Good 
				? TagQuality.Bad_LOCF
				: TagQuality.Good_LOCF);
	}
}
