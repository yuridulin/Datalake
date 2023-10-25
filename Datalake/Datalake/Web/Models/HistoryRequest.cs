using System;
using System.Collections.Generic;

namespace Datalake.Web.Models
{
	public class HistoryRequest
	{
		public List<string> Tags { get; set; } = new List<string>();

		public DateTime? Old { get; set; } = DateTime.Now;

		public DateTime? Young { get; set;} = DateTime.Now;

		public int Resolution { get; set; } = 0;

		public AggFunc Func { get; set; } = AggFunc.List;
	}
}
