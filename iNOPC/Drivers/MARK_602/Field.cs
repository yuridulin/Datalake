namespace iNOPC.Drivers.MARK_602
{
	public struct Field
	{
		public string Name { get; set; }

		public double Ratio { get; set; }

		public byte OperationCode { get; set; }

		public byte Channel { get; set; }

		public byte RequestCode { get; set; }
	}
}