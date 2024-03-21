using Microsoft.EntityFrameworkCore;

namespace DatalakeDb.Models
{
	[Keyless]
	public class TagInput
	{
		public int ResultTagId { get; set; } = 0;

		public int InputTagId { get; set; } = 0;

		public string VariableName { get; set; } = string.Empty;
	}
}
