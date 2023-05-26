namespace iNOPC.Drivers.MODBUS_TCP.Models
{
	public class Field
	{
		public string Name { get; set; } = "";

		public object Value { get; set; } = 0;

		public string Type { get; set; } = "";

		public string Address { get; set; } = "0x";

		public ushort AddressDec { get; set; } = 0;

		public float Scale { get; set; } = 1;

		public bool IsActive { get; set; } = false;

		public bool Checked { get; set; } = false;
	}
}