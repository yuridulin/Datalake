using Datalake.Database;
using Datalake.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Web.Api
{
	public class ConsoleController : Controller
	{
		public Result Tree()
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
						FullName = x.Name,
						Type = TreeType.Source,
						Items = GetSubItems(tags
							.Where(t => t.SourceId == x.Id)
							.Select(t => new TreeItem
							{
								Id = t.Id,
								Name = t.Name.Replace(x.Name + '.', ""),
								FullName = t.Name,
							})
							.ToList()),
					})
					.ToList();

				var withoutSource = tags
					.Where(t => !sources.Select(x => x.Id).Contains(t.SourceId))
					.Select(x => new TreeItem
					{
						Id= x.Id,
						Name = x.Name,
						FullName = x.Name,
					})
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

				return Data(tree);
			}

			List<TreeItem> GetSubItems(List<TreeItem> tags)
			{
				var subs = tags
					.GroupBy(x => x.Name.Contains('.') ? x.Name.Substring(0, x.Name.IndexOf('.')) : x.Name)
					.Select(g => new TreeItem
					{
						Name = g.Key,
						FullName = g.FirstOrDefault()?.FullName ?? g.Key,
					})
					.ToList();

				foreach (var item in subs)
				{
					var tag = tags.FirstOrDefault(x => x.Name == item.Name);

					var included = tags
						.Where(x => x.Name.StartsWith(item.Name + '.'))
						.Select(x => new TreeItem
						{
							Id = x.Id,
							Name = x.Name.Replace(item.Name + '.', ""),
							FullName = x.FullName,
						})
						.ToList();

					if (included.Count == 0)
					{
						item.Id = tag?.Id ?? item.Id;
						item.FullName = tag?.FullName ?? item.FullName;
						item.Type = TreeType.Tag;
					}
					else
					{
						item.FullName = tag?.FullName ?? item.FullName;
						item.FullName = item.FullName.Substring(0, item.FullName.LastIndexOf(item.Name)) + item.Name;
						item.Type = TreeType.TagGroup;
					}

					item.Name = tag?.Name ?? item.Name;
					item.Items = GetSubItems(included);
				}

				return subs;
			}
		}
	}
}
