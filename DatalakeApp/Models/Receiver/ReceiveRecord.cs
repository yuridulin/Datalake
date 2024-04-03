using DatalakeDatabase.Enums;

namespace DatalakeApp.Models.Receiver
{
	public class ReceiveRecord
	{
		public string Name { get; set; } = "";

		public object? Value { get; set; } = null;

		public TagType Type { get; set; }

		public ushort Quality { get; set; } = 0;
	}
}
