using Datalake.Database.Enums;
using LinqToDB.Mapping;
using System;

namespace Datalake.Database
{
	[Table(Name = "TagsLive")]
	public class TagLive
	{
		[Column]
		public int TagId { get; set; }

		[Column]
		public DateTime Date { get; set; }

		/// <summary>
		/// Текстовое представление значения
		/// </summary>
		[Column]
		public string Text { get; set; }

		/// <summary>
		/// Отмасштабированное числовое значение. Для дискретов записывается как 0 / 1.
		/// </summary>
		[Column]
		public decimal? Number { get; set; }

		/// <summary>
		/// Оригинальное числовое значение
		/// </summary>
		[Column]
		public decimal? Raw { get; set; }

		[Column]
		public TagQuality Quality { get; set; }

		/// <summary>
		/// Получение значения на основе указанного типа
		/// </summary>
		/// <param name="type">тип тега</param>
		/// <returns></returns>
		public object Value(TagType type)
		{
			switch (type)
			{
				case TagType.Number: return Number;
				case TagType.Boolean: return Number.HasValue && Number.Value != 0;
				default: return Text;
			}
		}
	}
}
