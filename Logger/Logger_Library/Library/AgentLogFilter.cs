using System;
using System.Linq;

namespace Logger.Library
{
	public class AgentLogFilter
	{
		public int Id { get; set; }

		public bool Allow { get; set; }

		public string[] Endpoints { get; set; } = new string[ 0 ];

		public string[] Journals { get; set; } = new string[ 0 ];

		public string[] Sources { get; set; } = new string[ 0 ];

		public int[] EventIds { get; set; } = new int[ 0 ];

		public string[] Categories { get; set; } = new string[ 0 ];

		public string[] Types { get; set; } = new string[ 0 ];

		// Проверка сообщения на подпадание под действие фильтра

		public bool Pass(AgentLog log)
		{
			bool[] checks = new bool[]{
				(Endpoints.Length == 0 || Endpoints.Contains(log.Endpoint)),
				(Journals.Length == 0 || Journals.Contains(log.Journal)),
				(Sources.Length == 0 || Sources.Contains(log.Source)),
				(Categories.Length == 0 || Categories.Contains(log.Category)),
				(Types.Length == 0 || Types.Contains(log.Type)),
				(EventIds.Length == 0 || EventIds.Contains(log.EventId))
			};

			#if DEBUG
			//Console.WriteLine("Endpoint: " + log.Endpoint + " = " + checks[ 0 ]);
			//Console.WriteLine("Endpoints: " + string.Join(" ", Endpoints));

			//Console.WriteLine("Journal: " + log.Journal + " = " + checks[ 1 ]);
			//Console.WriteLine("Journals: " + string.Join(" ", Journals));

			//Console.WriteLine("Source: " + log.Source + " = " + checks[ 2 ]);
			//Console.WriteLine("Sources: " + string.Join(" ", Sources));

			//Console.WriteLine("Category: " + log.Category + " = " + checks[ 3 ]);
			//Console.WriteLine("Categories: " + string.Join(" ", Categories));

			//Console.WriteLine("Type: " + log.Type + " = " + checks[ 4 ]);
			//Console.WriteLine("Types: " + string.Join(" ", Types));

			//Console.WriteLine("EventId: " + log.EventId + " = " + checks[ 5 ]);
			//Console.WriteLine("EventIds: " + string.Join(" ", EventIds));
			#endif

			return checks[ 0 ] && checks[ 1 ] && checks[ 2 ] && checks[ 3 ] && checks[ 4 ] && checks[ 5 ];
		}
	}
}
