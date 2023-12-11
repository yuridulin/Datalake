using Datalake.Enums;
using LinqToDB.Mapping;

namespace Datalake.Database.V0
{
	[Table(Name = "Tags")]
	public class Tag
	{
		[Column, PrimaryKey, Identity]
		public int Id { get; set; }

		[Column, NotNull]
		public string Name { get; set; } = string.Empty;

		[Column]
		public string Description { get; set; } = string.Empty;

		[Column, NotNull]
		public TagType Type { get; set; } = TagType.String;

		[Column, NotNull]
		public short Interval { get; set; } = 0;

		// для значений, получаемых из источника

		[Column, NotNull]
		public int SourceId { get; set; } = 0;

		[Column]
		public string SourceItem { get; set; } = string.Empty;

		// для числовых значений (шкалирование производится при записи нового значения)

		[Column, NotNull]
		public bool IsScaling { get; set; } = false;

		[Column, NotNull]
		public float MinEU { get; set; } = 0;

		[Column, NotNull]
		public float MaxEU { get; set; } = 100;

		[Column, NotNull]
		public float MinRaw { get; set; } = 0;

		[Column, NotNull]
		public float MaxRaw { get; set; } = 100;

		// для вычисляемых тегов (вычисление - в модуле CalculatorWorker)

		[Column, NotNull]
		public bool IsCalculating { get; set; } = false;

		[Column]
		public string Formula { get; set; } = string.Empty;
	}
}
