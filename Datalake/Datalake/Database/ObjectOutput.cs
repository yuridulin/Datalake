using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table(Name = "ObjectsOutputs")]
	public class ObjectOutput
	{
		public string Name { get; set; }

		public object Value { get; set; }

		public string Formula { get; set; }

		public object DefaultValue { get; set; }
	}
}
