using LinqToDB.Mapping;
using System.Collections.Generic;

namespace Datalake.Database
{
	[Table(Name = "Objects")]
	public class Object
	{
		[Column, PrimaryKey, Identity]
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
}
