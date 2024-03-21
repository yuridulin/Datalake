using DatalakeApp.Services;
using DatalakeDb;
using DatalakeDb.Classes;
using DatalakeDb.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.Controllers
{
	public class TagsController(DatalakeContext db, CacheService cache) : Controller
	{
		public IActionResult Index()
		{
			var tags = db.Tags
				.ToArray();

			return View(tags);
		}

		[HttpPost]
		public IActionResult Live([FromBody] LiveRequest request)
		{
			request.Tags ??= db.Tags
				.Where(x => request.TagNames.Count == 0 || request.TagNames.Contains(x.Name))
				.Select(x => x.Id)
				.ToList();

			var tags = request.Tags.Count == 0
				? db.Tags.ToList()
				: db.Tags.Where(x => request.Tags.Contains(x.Id)).ToList();

			var live = tags
				.Select(x => new
				{
					Tag = x,
					Value = cache.Read(x.Id)
				})
				.Select(x => new HistoryResponse
				{
					Id = x.Tag.Id,
					TagName = x.Tag.Name,
					Type = x.Tag.Type,
					Func = AggregationFunc.List,
					Values = new List<HistoryRecord>
					{
						new() {
							Date = x.Value.Date,
							Value = x.Value.Value(),
							Quality = x.Value.Quality,
							Using = x.Value.Using,
						}
					}
				})
				.ToList();
		}
	}
}
