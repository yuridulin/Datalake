using Datalake.Database.Enums;
using Datalake.Workers.Logs;
using Datalake.Workers.Logs.Models;
using LinqToDB.Mapping;
using NCalc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public float MinEU { get; set; } = 0;

		[Column, NotNull]
		public float MaxEU { get; set; } = 100;

		[Column, NotNull]
		public float MinRaw { get; set; } = 0;

		[Column, NotNull]
		public float MaxRaw { get; set; } = 100;


		// для вычисляемых тегов (вычисление - в модуле CalculatorWorker)

		[Column, NotNull]
		public bool IsCalculating { get; set; } = false;

		[Column]
		public string Formula { get; set; } = string.Empty;


		// логика обновления оригинального значения

		DateTime LastUpdate { get; set; } = DateTime.MinValue;

		public void PrepareToCollect()
		{
			LastUpdate = DateTime.MinValue;
			LogsWorker.Add("Collector", "PrepareToCollect: LastUpdate " + LastUpdate, LogType.Trace);
		}

		public bool IsNeedToUpdate(DateTime now)
		{
			LogsWorker.Add("Collector", "IsNeedToUpdate: now " + now + " LastUpdate " + LastUpdate + " interval " + Interval + " result " + (Interval <= 0 || ((now - LastUpdate).TotalSeconds >= Interval)), LogType.Trace);
			return Interval <= 0 || ((now - LastUpdate).TotalSeconds >= Interval);
		}

		public void SetAsUpdated(DateTime now)
		{
			LastUpdate = now;
			LogsWorker.Add("Collector", "PrepareToCollect: LastUpdate " + LastUpdate, LogType.Trace);
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
			catch (Exception ex)
			{
				LogsWorker.Add("Collector", "FromRaw: " + ex.Message, LogType.Trace);
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
