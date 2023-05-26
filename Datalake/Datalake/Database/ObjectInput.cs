using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table(Name = "ObjectsInputs")]
	public class ObjectInput
	{
		public string TagName { get; set; }

		public int InputType { get; set; }
	}
}
