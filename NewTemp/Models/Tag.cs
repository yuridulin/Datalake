using DatalakeDatabase.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models
{
	[Table(TableName), LinqToDB.Mapping.Table(TableName)]
	public class Tag
	{
		const string TableName = "Tags";

		// поля в БД

		[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Column]
		public Guid GlobalId { get; set; } = Guid.Empty;

		[Column]
		public required string Name { get; set; } = string.Empty;

		[Column]
		public required string Description { get; set; } = string.Empty;

		[Column]
		public TagType Type { get; set; }

		[Column, NotNull]
		public DateTime Created { get; set; }


		[Column]
		public int SourceId { get; set; } = 0;

		[Column, Nullable]
		public string? SourceItem {  get; set; } = string.Empty;


		[Column]
		public bool IsScaling { get; set; } = false;

		[Column]
		public float MinEU { get; set; } = float.MinValue;

		[Column]
		public float MaxEU { get; set; } = float.MaxValue;

		[Column]
		public float MinRaw { get; set; } = float.MinValue;

		[Column]
		public float MaxRaw { get; set; } = float.MaxValue;

		// связи

		[ForeignKey(nameof(SourceId))]
		public Source Source { get; set; } = null!;

		[NotMapped]
		public ICollection<TagInput> Inputs { get; set; } = [];

		public ICollection<EntityTag> RelatedEntities { get; set; } = [];

		public ICollection<Entity> Entities { get; set; } = [];

		// реализация

		public (string?, float?, TagQuality) FromRaw(object? value, ushort quality = 192)
		{
			try
			{
				string? text = null;
				float? number = null;

				if (Type == TagType.String)
				{
					if (value is string v)
					{
						text = v;
						number = null;
					}
				}

				if (Type == TagType.Boolean)
				{
					if (value is bool v)
					{
						text = v ? "true" : "false";
						number = v ? 1 : 0;
					}
				}

				if (Type == TagType.Number)
				{
					float? raw = null;
					if (float.TryParse(value?.ToString(), out float d))
					{
						raw = d;
					}

					number = raw;

					// вычисление значения на основе шкалирования
					if (Type == TagType.Number && raw.HasValue && IsScaling)
					{
						number = raw.Value * ((MaxEU - MinEU) / (MaxRaw - MinRaw));
					}
				}

				TagQuality tagQuality = !Enum.IsDefined(typeof(TagQuality), (int)quality)
					? TagQuality.Unknown
					: (TagQuality)quality;

				return (text, number, tagQuality);
			}
			catch (Exception)
			{
				return (null, null, TagQuality.Unknown);
			}
		}
	}
}
