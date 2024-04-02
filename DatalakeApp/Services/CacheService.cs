using DatalakeDatabase.Models;

namespace DatalakeApp.Services
{
	public class CacheService
	{
		public Dictionary<int, TagHistory> LiveValues { get; set; } = [];

		public DateTime LastUpdate { get; set; } = DateTime.MinValue;

		public CacheService()
		{
			// инициализация из БД ?
		}

		public void WriteValue(TagHistory value)
		{
			lock (LiveValues)
			{
				UnsafeWriteValue(value);
				LastUpdate = DateTime.UtcNow;
			}
		}

		public void WriteValues(TagHistory[] values)
		{
			lock (LiveValues)
			{
				foreach (var value in values)
				{
					UnsafeWriteValue(value);
				}
				LastUpdate = DateTime.UtcNow;
			}
		}

		void UnsafeWriteValue(TagHistory value)
		{
			if (LiveValues.TryGetValue(value.TagId, out var exist))
			{
				if (exist.Date < value.Date)
				{
					exist.Date = value.Date;
					exist.Number = value.Number;
					exist.Text = value.Text;
					exist.Quality = value.Quality;
					exist.Using = value.Using;
				}
			}
			else
			{
				LiveValues[value.TagId] = new TagHistory
				{
					Date = value.Date,
					Number = value.Number,
					Text = value.Text,
					Quality = value.Quality,
					TagId = value.TagId,
					Using = value.Using,
				};
			}
		}

		public TagHistory? ReadValue(int id)
		{
			lock (LiveValues)
			{
				return LiveValues.TryGetValue(id, out var exist) ? exist : null;
			}
		}

		public TagHistory[] ReadValues(int[] id)
		{
			lock (LiveValues)
			{
				return LiveValues
					.Where(x => id.Contains(x.Key))
					.Select(x => x.Value)
					.ToArray();
			}
		}
	}
}
