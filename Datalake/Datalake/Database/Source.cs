using Datalake.Database.Enums;
using Datalake.Web;
using Datalake.Workers;
using LinqToDB.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Database
{
	[Table(Name = "Sources")]
	public class Source
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column]
		public string Name { get; set; }

		[Column]
		public SourceType Type { get; set; }

		[Column]
		public string Address { get; set; }

		// реализация

		public List<string> GetItems()
		{
			if (Type == SourceType.Inopc)
			{
				var res = Inopc.AskInopc(new string[0], Address);

				return res.Tags
					.Select(x => x.Name)
					.OrderBy(x => x)
					.ToList();
			}
			else if (Type == SourceType.Datalake)
			{
				var res = Inopc.AskInopc(new string[0], Address);

				return res.Tags
					.Select(x => x.Name)
					.OrderBy(x => x)
					.ToList();
			}
			else
			{
				return new List<string>();
			}
		}
	}
}
