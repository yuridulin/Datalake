namespace MR_TEST
{
    public class Field
    {
        public object Value { get; set; }

        public string Name { get; set; }

        public ushort Address { get; set; }

        public FieldType Type { get; set; } = FieldType.BIT;

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