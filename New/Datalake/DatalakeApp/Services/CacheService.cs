using DatalakeDb.Models;
using System.Globalization;

namespace DatalakeApp.Services
{
	public class CacheService
	{
		readonly string HistoryTablePrefix = "History_";

		List<string> Tables = [];

		Dictionary<uint, History> Live = [];

		DateTime LastUpdate = DateTime.MinValue;


		public bool IsTableExists(DateTime date) => Tables.Contains(GetTableName(date));

		public string GetTableName(DateTime date) => HistoryTablePrefix + date.ToString("yyyy_MM_dd");

		public List<DateTime> GetStoredDays()
		{
			return Tables
				.Select(x => DateTime.ParseExact(x.Replace(HistoryTablePrefix, ""), "yyyy_MM_dd", new CultureInfo("ru-RU")))
				.ToList();
		}

		public DateTime Last() => LastUpdate;

		public void Update() => LastUpdate = DateTime.Now;

		public History Read(uint id)
		{
			lock (Live)
			{
				return Live[id];
			}
		}

		public bool IsNew(History value)
		{
			var old = Read(value.TagId);

			return (old.Quality != value.Quality) || (old.Text != value.Text) || (old.Number != value.Number);
		}

		public void Write(History value)
		{
			lock (Live)
			{
				if (!Live.TryGetValue(value.TagId, out History? h))
				{
					Live.Add(value.TagId, value);
				}
				else if (h.Date <= value.Date)
				{
					Live[h.TagId] = value;
				}
			}
		}

		public void WriteMany(List<History> values)
		{
			lock (Live)
			{
				foreach (var value in values)
				{
					if (!Live.TryGetValue(value.TagId, out History? h))
					{
						Live.Add(value.TagId, value);
					}
					else if (h.Date <= value.Date)
					{
						Live[h.TagId] = value;
					}
				}
			}
		}
	}
}
