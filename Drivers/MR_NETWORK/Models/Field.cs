namespace iNOPC.Drivers.MR_NETWORK.Models
{
    public class Field
    {
        public object Value { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; } = "0x0000";

        public ushort Hex { get; set; } = 0;

        public FieldType Type { get; set; } = FieldType.BIT;

        public void ParseHex()
        {
            if (Address != "0x0000")
            {
                Hex = ushort.Parse(Address.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            }
        }

        public byte GetBytesLength()
        {
            switch (Type)
            {
                case FieldType.DATE: return 7;
                case FieldType.WORD: return 2;
                case FieldType.BIT: return 1;
                default: return 1;
            }
        }

        public byte GetReadCommand()
        {
            switch (Type)
            {
                case FieldType.DATE: return 3;
                case FieldType.WORD: return 3;
                default: return 2;
            }
        }
    }

    public enum FieldType
    {
        DATE,
        BIT,
        WORD,
    }
}