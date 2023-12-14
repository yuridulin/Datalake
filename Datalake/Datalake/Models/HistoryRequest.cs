using Datalake.Enums;
using System;

namespace Datalake.Models
{
	public class HistoryRequest : LiveRequest
	{
		public DateTime? Old { get; set; }

		public DateTime? Young { get; set; }

		public DateTime? Exact { get; set; }

		public int Resolution { get; set; } = 0;

		public AggFunc Func { get; set; } = AggFunc.List;
	}
}
