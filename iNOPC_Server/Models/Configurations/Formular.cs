using iNOPC.Server.Storage;
using NCalc;
using System;
using System.Collections.Generic;

namespace iNOPC.Server.Models.Configurations
{
	public class Formular
	{
		public string Name { get; set; } = "";

		public string Formula { get; set; } = "";

		public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();

		public string Description { get; set; } = "";

		public int Interval { get; set; } = 1;


		public object Value { get; set; } = 0;

		public string Error { get; set; } = "";

		public DateTime LastCalc { get; set; }

		public void Set()
		{
			if (!string.IsNullOrEmpty(Formula))
			{
				Expression = new Expression(Formula);
				Expression.EvaluateParameter += (name, args) =>
				{
					if (Fields.ContainsKey(name))
					{
						// переменная определена
						args.Result = OPC.Read(Fields[name]) ?? 0;
					}
					else
					{
						// переменная не определена
						args.Result = 0;
					}

					Program.Log("Параметр " + name + " = " + args.Result);
				};
			}
			else
			{
				Expression = null;
			}

			OPC.Write(Name, 0, 0);

			LastCalc = DateTime.Parse("01.01.2000");
		}

		public void Calculate(DateTime date)
		{
			Program.Log("Расчёт тега " + Name);

			if ((date - LastCalc).TotalSeconds < Interval) return;

			object result;
			ushort quality = 192;

			if (Expression == null)
			{
				Error = "Формула пуста";
				result = 0;
				quality = 0;
			}
			else 
			{ 
				try
				{
					result = Expression.Evaluate();
					Error = "";

					if (Expression.HasErrors())
					{
						Error = Expression.Error;
						result = 0;
						quality = 0;
					}
					else if (result.GetType() == typeof(string))
					{
						Error = result.ToString();
						result = 0;
						quality = 0;
					}
				}
				catch (Exception e)
				{
					Error = e.Message;
					result = 0;
					quality = 0;
				}
			}

			LastCalc = date;
			Value = result;

			Program.Log("Значение тега " + Value + ", тип " + Value.GetType().ToString());

			OPC.Write(Name, result, quality);
		}

		Expression Expression { get; set; }
	}
}