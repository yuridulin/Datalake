using iNOPC.Server.Models;
using iNOPC.Server.Models.Configurations;
using iNOPC.Server.Storage;
using iNOPC.Server.Web.RequestTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iNOPC.Server.Web.Api
{
	public class FormularsController : Controller
	{
		public object Fields()
		{
			lock (Program.Configuration.Formulars)
			{
				return Program.Configuration.Formulars
					.Select(x => new
					{
						x.Name,
						x.Description,
						x.Formula,
						x.Interval,
						x.Fields,
					});
			}
		}

		public object Values()
		{
			lock (Program.Configuration.Formulars)
			{
				return Program.Configuration.Formulars
					.Select(x => new
					{
						x.Name,
						x.Value,
						x.Error,
					});
			}
		}

		public object Form(FormulaForm form)
		{
			try
			{
				lock (OPC.Tags)
				{
					return OPC.Tags
						.Select(x => x.Key)
						.Where(x => x != form.Name)
						.OrderBy(x => x)
						.ToList();
				}
			}
			catch (Exception e)
			{
				return new { Error = e.Message };
			}
		}

		public object Create()
		{
			try
			{
				lock (Program.Configuration.Formulars)
				{
					var f = new Formular
					{
						Name = "Field" + new Random().Next(),
						Interval = 5,
						Fields = new Dictionary<string, string>(),
					};

					while (Program.Configuration.Formulars.Count(x => x.Name == f.Name) > 0 || OPC.Tags.ContainsKey(f.Name))
					{
						f.Name = "Field" + new Random().Next();
					}

					f.Set();
					Program.Configuration.Formulars.Add(f);
				}

				Program.Configuration.SaveToFile();
				Calculator.Reset();

				return new { Done = true };
			}
			catch (Exception e)
			{
				return new { Error = e.Message };
			}
		}

		public object Delete(FormulaForm form)
		{
			try
			{
				lock (Program.Configuration.Formulars)
				{
					var field = Program.Configuration.Formulars
						.FirstOrDefault(x => x.Name == form.Name);

					if (field == null) return new { Error = "Поле с таким именем не найдено" };

					Program.Configuration.Formulars.Remove(field);
					OPC.Remove(form.Name);
				}

				Program.Configuration.SaveToFile();
				Calculator.Reset();

				return new { Done = true };
			}
			catch (Exception e)
			{
				return new { Error = e.Message };
			}
		}

		public object Update(FormulaForm form)
		{
			try
			{
				lock (Program.Configuration.Formulars)
				{
					var field = Program.Configuration.Formulars
						.FirstOrDefault(x => x.Name == form.OldName);

					if (field == null)
						return new { Error = "Тег не найден" };
					if (form.OldName != form.Name && OPC.Tags.ContainsKey(form.Name))
						return new { Error = "Тег с таким именем уже существует" };
					if (form.Interval <= 0)
						return new { Error = "Интервал расчёта должен быть положительным кол-вом секунд" };

					field.Name = form.Name;
					field.Description = form.Description;
					field.Formula = form.Formula;
					field.Interval = form.Interval;
					field.Fields = form.Fields;

					if (form.OldName != form.Name)
					{
						OPC.Remove(form.OldName);
					}
				}

				Program.Configuration.SaveToFile();
				Calculator.Reset();

				return new { Done = true };
			}
			catch (Exception e)
			{
				return new { Error = e.Message };
			}
		}
	}
}
