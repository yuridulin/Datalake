namespace MODBUS_RTU_over_TCP.Models
{
	public class PackagePart
	{
		public string FieldName { get; set; } = "NotSet";

		public string Type { get; set; } = "";

		public byte Length { get; set; } = 0;

		public object Value { get; set; } = 0;
	}
}