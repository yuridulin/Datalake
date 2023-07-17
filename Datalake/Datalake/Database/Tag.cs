using Datalake.Database.Enums;
using LinqToDB.Mapping;
using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Database
{
	[Table(Name = "Tags")]
	public class Tag
	{
		[Column, PrimaryKey, Identity]
		public int Id { get; set; }

		[Column, NotNull]
		public string Name { get; set; } = string.Empty;

		[Column]
		public string Description { get; set; } = string.Empty;

		[Column, NotNull]
		public TagType Type { get; set; } = TagType.String;

		[Column, NotNull]
		public short Interval { get; set; } = 0;


		// для значений, получаемых из источника

		[Column, NotNull]
		public int SourceId { get; set; } = 0;

		[Column]
		public string SourceItem { get; set; } = string.Empty;


		// для числовых значений (шкалирование производится при записи нового значения)

		[Column, NotNull]
		public bool IsScaling { get; set; } = false;

		[Column, NotNull]
		public decimal MinEU { get; set; } = 0;

		[Column, NotNull]
		public decimal MaxEU { get; set; } = 100;

		[Column, NotNull]
		public decimal MinRaw { get; set; } = 0;

		[Column, NotNull]
		public decimal MaxRaw { get; set; } = 100;


		// для вычисляемых тегов (вычисление - в модуле CalculatorWorker)

		[Column, NotNull]
		public bool IsCalculating { get; set; } = false;

		[Column]
		public string Formula { get; set; } = string.Empty;


		// логика обновления оригинального значения

		TimeSpan UpdateInterval = TimeSpan.Zero;

		DateTime LastUpdate = DateTime.MinValue;

		public void PrepareToCollect()
		{
			UpdateInterval = TimeSpan.FromSeconds(Interval);
			LastUpdate = DateTime.MinValue;
		}

		public bool IsNeedToUpdate(DateTime now)
		{
			return UpdateInterval == TimeSpan.Zero || (now - LastUpdate >= UpdateInterval);
		}

		public void SetAsUpdated(DateTime now)
		{
			LastUpdate = now;
		}

		public (string, decimal?, decimal?, TagQuality) FromRaw(object value, ushort quality)
		{
			string text = value?.ToString();

			decimal? raw = null;
			if (decimal.TryParse(value?.ToString(), out decimal d))
			{
				raw = d;
			}

			// вычисление значения на основе шкалирования
			decimal? number = raw;
			if (Type == TagType.Number && raw.HasValue && IsScaling)
			{
				number = raw.Value * ((MaxEU - MinEU) / (MaxRaw - MinRaw));
			}

			TagQuality tagQuality = !Enum.IsDefined(typeof(TagQuality), (int)quality)
				? TagQuality.Unknown
				: (TagQuality)quality;

			return (text, raw, number, tagQuality);
		}


		// логика обновления вычисляемого значения

		public List<Rel_Tag_Input> Inputs { get; set; } = new List<Rel_Tag_Input>();

		Expression Expression { get; set; }

		public void PrepareToCalc()
		{
			if (!string.IsNullOrEmpty(Formula))
			{
				Expression = new Expression(Formula);
				Expression.EvaluateParameter += (name, args) =>
				{
					var input = Inputs.FirstOrDefault(x => x.VariableName == name);
					if (input != null)
					{
						// переменная определена
						args.Result = Cache.Read(input.InputTagId) ?? 0;
					}
					else
					{
						// переменная не определена
						args.Result = 0;
					}
				};
			}
			else
			{
				Expression = null;
			}
		}

		public (string, decimal?, decimal?, TagQuality) Calculate()
		{
			object result;
			ushort quality = 0;
			string err;

			if (Expression == null)
			{
				err = "Формула пуста";
				result = 0;
				quality = 0;
			}
			else
			{
				try
				{
					result = Expression.Evaluate();
					err = "";

					if (Expression.HasErrors())
					{
						err = Expression.Error;
						result = 0;
						quality = 0;
					}
					else if (result.GetType() == typeof(string))
					{
						err = result.ToString();
						result = 0;
						quality = 0;
					}
				}
				catch (Exception e)
				{
					err = e.Message;
					result = 0;
					quality = 0;
				}
			}

			if (string.IsNullOrEmpty(err)) Console.WriteLine(err);
			return FromRaw(result, quality);
		}
	}
}
