using Datalake.Enums;
using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Database
{
	public class Tag : V0.Tag
	{
		// логика обновления оригинального значения

		DateTime LastUpdate { get; set; } = DateTime.MinValue;

		public void PrepareToCollect()
		{
			LastUpdate = DateTime.MinValue;
		}

		public bool IsNeedToUpdate(DateTime now)
		{
			return Interval <= 0 || ((now - LastUpdate).TotalSeconds >= Interval);
		}

		public void SetAsUpdated(DateTime now)
		{
			LastUpdate = now;
		}

		public (string, float?, TagQuality) FromRaw(object value, ushort quality)
		{
			try
			{
				string text = null;
				float? number = null;

				if (Type == TagType.String)
				{
					if (value is string v)
					{
						text = v;
						number = null;
					}
				}

				if (Type == TagType.Boolean)
				{
					if (value is bool v)
					{
						text = v ? "true" : "false";
						number = v ? 1 : 0;
					}
				}

				if (Type == TagType.Number)
				{
					float? raw = null;
					if (float.TryParse(value?.ToString(), out float d))
					{
						raw = d;
					}

					number = raw;

					// вычисление значения на основе шкалирования
					if (Type == TagType.Number && raw.HasValue && IsScaling)
					{
						number = raw.Value * ((MaxEU - MinEU) / (MaxRaw - MinRaw));
					}
				}

				TagQuality tagQuality = !Enum.IsDefined(typeof(TagQuality), (int)quality)
					? TagQuality.Unknown
					: (TagQuality)quality;

				return (text, number, tagQuality);
			}
			catch (Exception)
			{
				return (null, null, TagQuality.Unknown);
			}
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
						args.Result = Cache.Read(input.InputTagId)?.Value() ?? 0;
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

		public (string, float?, TagQuality) Calculate()
		{
			object result;
			ushort quality = 192;
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
