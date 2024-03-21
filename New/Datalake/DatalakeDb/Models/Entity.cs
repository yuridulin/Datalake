using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatalakeDb.Models
{
	public class Entity
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int ParentId { get; set; } = 0;

		public Guid GlobalId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;
	}
}
