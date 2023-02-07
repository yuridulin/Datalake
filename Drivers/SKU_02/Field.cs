namespace iNOPC.Drivers.SKU_02
{
	internal class Field
	{
		public string Name { get; set; }

		public int Start { get; set; }

		public bool IsFloat { get; set; }

		public float Demiliter { get; set; } = 1;
	}
}
