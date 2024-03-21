using DatalakeDb.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatalakeDb.Models
{
	[Keyless, NotMapped]
	public class History
	{
		public uint TagId { get; set; }

		public DateTime Date { get; set; } = DateTime.Now;

		public string Text { get; set; } = string.Empty;

		public float? Number { get; set; } = null;

		public TagQuality Quality { get; set; } = TagQuality.Good;

		public TagUsing Using { get; set; } = TagUsing.Basic;
	}
}
