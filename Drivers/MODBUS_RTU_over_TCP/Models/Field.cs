namespace MODBUS_RTU_over_TCP.Models
{
	public class Field
    {
        public string Name { get; set; } = "";

        public object Value { get; set; } = 0;

        public string Type { get; set; } = "";

        public ushort Address { get; set; } = 0;

        public string HexAddress { get; set; } = "0x0000";

        public byte CommandCode { get; set; } = 0;

        public bool Checked { get; set; } = false;
    }
}