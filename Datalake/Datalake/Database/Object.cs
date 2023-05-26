using Datalake.Database.Enums;
using LinqToDB.Mapping;
using System.Collections.Generic;

namespace Datalake.Database
{
	[Table(Name = "Objects")]
	public class Object
	{
		[Column, PrimaryKey]
		public int Id { get; set; } = 0;

		[Column]
		public string Name { get; set; } = string.Empty;

		public List<ObjectInput> Inputs { get; set; } = new List<ObjectInput>();

		public List<ObjectOutput> Outputs { get; set; } = new List<ObjectOutput>();

		public void Calculate()
		{
			// получаем инпуты
			// делаем вычисление формул на каждое выходное значение
			// обрабатываем результаты
		}
	}

	[Table(Name = "ObjectsInputs")]
	public class ObjectInput
	{
		public string TagName { get; set; }

		public int InputType { get; set; }
	}

	[Table(Name = "ObjectsOutputs")]
	public class ObjectOutput
	{
		public string Name { get; set; }

		public object Value { get; set; }

		public string Formula { get; set; }

		public object DefaultValue { get; set; }
	}
}
