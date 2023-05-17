using LinqToDB.Mapping;
using System.Linq;

namespace Logger.Database
{
	[Table(Name = "Filters")]
	public class Filter
	{
		[Column, Identity]
		public int Id { get; set; }

		[Column, NotNull]
		public string Name { get; set; }

		[Column]
		public string Description { get; set; }

		[Column, NotNull]
		public bool IsAllowing { get; set; }

		[Column, NotNull]
		public string Computers { get; set; }

		[Column, NotNull]
		public string Journals { get; set; }

		[Column]
		public string Categories { get; set; }

		[Column, NotNull]
		public string Types { get; set; }

		[Column, NotNull]
		public string EventIds { get; set; }

		[Column]
		public string Sources { get; set; }

		[Column]
		public string Usernames { get; set; }


		public string[] ComputersArray
		{
			get
			{
				return Computers.Split(',');
			}
			set
			{
				if (value == null)
				{
					Computers = string.Join(",", value);
				}
			}
		}

		public string[] JournalsArray
		{
			get
			{
				return Journals.Split(',');
			}
			set
			{
				if (value == null)
				{
					Journals = string.Join(",", value);
				}
			}
		}

		public string[] CategoriesArray
		{
			get
			{
				return Categories.Split(',');
			}
			set
			{
				if (value == null)
				{
					Categories = string.Join(",", value);
				}
			}
		}

		public int[] TypesArray
		{
			get
			{
				return Types.Split(',').Select(x => int.Parse(x)).ToArray();
			}
			set
			{
				if (value == null)
				{
					Types = string.Join(",", value);
				}
			}
		}

		public int[] EventIdsArray
		{
			get
			{
				return EventIds.Split(',').Select(x => int.Parse(x)).ToArray();
			}
			set
			{
				if (value == null)
				{
					EventIds = string.Join(",", value);
				}
			}
		}

		public string[] SourcesArray
		{
			get
			{
				return Sources.Split(',');
			}
			set
			{
				if (value == null)
				{
					Sources = string.Join(",", value);
				}
			}
		}

		public string[] UsernamesArray
		{
			get
			{
				return Usernames.Split(',');
			}
			set
			{
				if (value == null)
				{
					Usernames = string.Join(",", value);
				}
			}
		}
	}
}
