namespace iNOPC.Drivers.MODBUS_TCP.Models
{
	public class PackagePart
	{
		public string FieldName { get; set; } = "NotSet";

		public string Type { get; set; } = "";

		public float Scale { get; set; } = 1;

		public byte Length { get; set; } = 0;

		public object Value { get; set; } = 0;
	}
}