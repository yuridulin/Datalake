using System.Collections.Generic;

namespace Datalake.Models
{
	public class LiveRequest
	{
		public List<int> Tags { get; set; } = new List<int>();

		public List<string> TagNames { get; set; } = new List<string>();
	}
}
