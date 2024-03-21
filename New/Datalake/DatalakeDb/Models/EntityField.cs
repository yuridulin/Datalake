using DatalakeDb.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatalakeDb.Models
{
	public class EntityField
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int EntityId { get; set; } = 0;

		public string Name { get; set; } = string.Empty;

		public string Value { get; set; } = string.Empty;

		public TagType Type { get; set; } = TagType.String;
	}
}
