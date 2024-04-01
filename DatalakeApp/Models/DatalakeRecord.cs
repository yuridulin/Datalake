using DatalakeDatabase.Enums;

namespace DatalakeApp.Models
{
	public class DatalakeRecord
	{
		public string Name { get; set; } = "";

		public object? Value { get; set; } = null;

		public TagType Type { get; set; }

		public ushort Quality { get; set; } = 0;
	}
}
