using DatalakeDb.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatalakeDb.Models
{
	public class Source
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public SourceType Type { get; set; } = SourceType.Inopc;

		public string Address { get; set; } = string.Empty;
	}
}
