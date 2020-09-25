namespace iNOPC.Drivers.MODBUS_TCP.Models
{
    public class Field
    {
        public string Name { get; set; } = "";

        public object Value { get; set; } = 0;

        public string Type { get; set; } = "";

        public ushort Address { get; set; } = 0;

        public ushort Scale { get; set; } = 1;

        public bool Checked { get; set; } = false;
    }
}