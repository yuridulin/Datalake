using Datalake.Enums;
using Datalake.Models;
using Datalake.Web;
using LinqToDB.Mapping;
using System;
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

		public List<Tag> Tags { get; set; } = new List<Tag>();

		public DatalakeResponse GetItems(DatabaseContext db)
		{
			DatalakeResponse res;

			switch (Type)
			{
				case SourceType.Inopc:
					res = Collector.AskInopc(db, new string[0], Address);
					break;

				case SourceType.Datalake:
					res = Collector.AskDatalake(db, new string[0], Address);
					break;

				default:
					res = new DatalakeResponse { Tags = new InputTag[0] };
					break;
			}

			return res;
		}

		public void Rebuild(DatabaseContext db)
		{
			Tags = db.Tags
				.Where(x => Id == x.SourceId)
				.Where(x => x.Type == TagType.String || x.Type == TagType.Number || x.Type == TagType.Boolean)
				.ToList();

			foreach (var tag in Tags)
			{
				tag.PrepareToCollect();
			}
		}

		public void Update(DatabaseContext db)
		{
			var now = DateTime.Now;

			var tagsToUpdate = Tags
				.Where(x => x.IsNeedToUpdate(now))
				.ToList();

			if (tagsToUpdate.Count == 0) return;

			DatalakeResponse res;

			switch (Type)
			{
				case SourceType.Inopc:
					res = Collector.AskInopc(db, tagsToUpdate.Select(x => x.SourceItem).ToArray(), Address);
					break;

				case SourceType.Datalake:
					res = Collector.AskDatalake(db, tagsToUpdate.Select(x => x.SourceItem).ToArray(), Address);
					break;

				default:
					tagsToUpdate.ForEach(tag => tag.SetAsUpdated(now));
					return;
			}

			var values = new List<TagHistory>();

			foreach (var tag in tagsToUpdate)
			{
				TagHistory value;
				var input = res.Tags.FirstOrDefault(x => x.Name == tag.SourceItem);

				if (input != null)
				{
					var (text, number, quality) = tag.FromRaw(input.Value, input.Quality);

					value = new TagHistory
					{
						TagId = tag.Id,
						Date = res.Timestamp,
						Text = text,
						Number = number,
						Quality = quality,
						Type = tag.Type,
						Using = TagHistoryUse.Basic,
					};
				}
				else
				{
					value = new TagHistory
					{
						TagId = tag.Id,
						Date = res.Timestamp,
						Text = null,
						Number = null,
						Quality = TagQuality.Bad_NoConnect,
						Type = tag.Type,
						Using = TagHistoryUse.Basic,
					};
				}

				if (Cache.IsNew(value)) values.Add(value);
			}

			if (values.Count > 0)
			{
				db.WriteHistory(values);
				db.Log(new Log
				{
					Category = LogCategory.Collector,
					Type = LogType.Trace,
					Ref = Id,
					Text = $"Новых значений: {values.Count}",
				});
			}

			foreach (var tag in tagsToUpdate)
			{
				tag.SetAsUpdated(now);
			}
		}
	}
}
