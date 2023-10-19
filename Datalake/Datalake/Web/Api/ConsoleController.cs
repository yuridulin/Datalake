using Datalake.Database;
using Datalake.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Web.Api
{
	public class ConsoleController : Controller
	{
		public object Tree()
		{
			using (var db = new DatabaseContext())
			{
				var tags = db.Tags
					.ToList();

				var sources = db.Sources
					.ToList();

				var tree = sources
					.Select(x => new TreeItem
					{
						Id = x.Id,
						Name = x.Name,
						Type = TreeType.Source,
						Items = GetSubItems(tags
							.Where(t => t.SourceId == x.Id)
							.Select(t => new Tag
							{
								Id = t.Id,
								Name = t.Name.Replace(x.Name + '.', ""),
							})
							.ToList()),
					})
					.ToList();

				var withoutSource = tags
					.Where(t => !sources.Select(x => x.Id).Contains(t.SourceId))
					.ToList();

				if (withoutSource.Count > 0)
				{
					tree.Add(new TreeItem
					{
						Id = 0,
						Name = "Источник не найден",
						Type = TreeType.Source,
						Items = GetSubItems(withoutSource),
					});
				}

				return tree;
			}

			List<TreeItem> GetSubItems(List<Tag> tags)
			{
				var subs =  tags
					.Select(x => x.Name.Contains('.') ? x.Name.Substring(0, x.Name.IndexOf('.')) : x.Name)
					.Distinct()
					.Select(x => new TreeItem
					{
						Name = x,
					})
					.ToList();

				foreach (var item in subs)
				{
					var included = tags
						.Where(x => x.Name.StartsWith(item.Name + '.'))
						.Select(x => new Tag
						{
							Id = x.Id,
							Name = x.Name.Replace(item.Name + '.', ""),
						})
						.ToList();

					if (included.Count == 0)
					{
						item.Id = tags.FirstOrDefault(x => x.Name == item.Name).Id;
						item.Type = TreeType.Tag;
					}
					else
					{
						item.Type = TreeType.TagGroup;
					}

					item.Items = GetSubItems(included);
				}

				return subs;
			}
		}
	}
}
