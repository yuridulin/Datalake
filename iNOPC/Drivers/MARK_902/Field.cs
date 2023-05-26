namespace iNOPC.Drivers.MARK_902
{
    public struct Field
    {
        public string Name { get; set; }

        public byte OperationCode { get; set; }

        public byte Channel { get; set; }

        public byte RequestCode { get; set; }

        public float Ratio { get; set; }

        public bool OnlySecondByte { get; set; }
    }
}