using Datalake.Enums;

namespace Datalake.Models
{
	public class InputTag
	{
		public string Name { get; set; } = "";

		public object Value { get; set; } = null;

		public TagType Type { get; set; }

		public ushort Quality { get; set; } = 0;
	}
}
