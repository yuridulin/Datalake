namespace iNOPC.Drivers.MODBUS_RTU.Models
{
	public class Field
	{
		public string Name { get; set; } = "";

		public object Value { get; set; } = 0;

		public string Type { get; set; } = "Single";

		public ushort Address { get; set; } = 0;

		public string HexAddress { get; set; } = "0x0000";

		public byte CommandCode { get; set; } = 3;

		public bool Checked { get; set; } = false;

		public bool IsActive { get; set; } = false;

		public string Description { get; set; } = "";

		public float Scale { get; set; } = 1;
	}
}