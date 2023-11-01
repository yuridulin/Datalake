using System;
using System.Collections.Generic;

namespace Datalake.Web.Models
{
	public class LiveRequest
	{
		public List<int> Tags { get; set; } = new List<int>();

		public List<string> TagNames { get; set; } = new List<string>();
	}
}
