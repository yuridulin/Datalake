using Datalake.Database;
using Datalake.Database.Enums;
using Datalake.Web.Models;
using Datalake.Workers.Logs;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datalake.Web.Api
{
	public class TagsController : Controller
	{
		public object List(string[] names = null)
		{
			using (var db = new DatabaseContext())
			{
				var sources = db.Sources.ToList();

				return db.Tags
					.Where(x => names == null || names.Contains(x.Name))
					.Select(x => new
					{
						x.Id,
						x.Name,
						x.Type,
						x.Description,
						x.SourceId,
						x.SourceItem,
						x.Interval,
						Source = sources.DefaultIfEmpty(new Source { Name = "?" }).FirstOrDefault(s => s.Id == x.SourceId).Name,
					})
					.OrderBy(x => x.Name)
					.ToList();
			}
		}

		public object Types()
		{
			return Enum.GetValues(typeof(TagType)).Cast<TagType>().ToDictionary(x => (int)x, x => x.ToString());
		}

		public object Inputs(int id)
		{
			using (var db = new DatabaseContext())
			{
				var outputs = db.Rel_Tag_Input
					.Where(x => x.InputTagId == id)
					.Select(x => x.TagId)
					.ToList();

				return db.Tags
					.Where(x => x.Id != id)
					.Where(x => !outputs.Contains(x.Id))
					.Select(x => new { x.Id, x.Name })
					.ToList();
			}
		}

		public List<HistoryResponse> Live(LiveRequest request)
		{
			using (var db = new DatabaseContext())
			{
				request.Tags = request.Tags ?? db.Tags
					.Where(x => request.TagNames.Count == 0 || request.TagNames.Contains(x.Name))
					.Select(x => x.Id)
					.ToList();

				var tags = request.Tags.Count == 0
					? db.Tags.ToList()
					: db.Tags.Where(x => request.Tags.Contains(x.Id)).ToList();

				return tags
					.Select(x => new
					{
						Tag = x,
						Value = Cache.Read(x.Id),
					})
					.Select(x => new HistoryResponse
					{
						Id = x.Tag.Id,
						TagName = x.Tag.Name,
						Type = x.Tag.Type,
						Func = AggFunc.List,
						Values = new List<HistoryValue>
						{
							new HistoryValue
							{
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

		public List<HistoryResponse> History(List<HistoryRequest> request)
		{
			using (var db = new DatabaseContext())
			{
				var response = new List<HistoryResponse>();

				foreach (var set in request)
				{
					if (!set.Old.HasValue && !set.Young.HasValue)
					{
						response.AddRange(Live(set));
					}
					else
					{
						set.Tags = set.Tags ?? db.Tags
							.Where(x => set.TagNames.Count == 0 || set.TagNames.Contains(x.Name))
							.Select(x => x.Id)
							.ToList();

						var tags = set.Tags.Count == 0
							? db.Tags.ToList()
							: db.Tags.Where(x => set.Tags.Contains(x.Id)).ToList();

						var young = set.Young ?? DateTime.Now;
						var old = set.Old ?? young.Date;

						var data = db.ReadHistory(tags.Select(x => x.Id).ToArray(), old, young, Math.Max(0, set.Resolution));

						foreach (var item in data.GroupBy(x => x.TagId))
						{
							var tag = tags.FirstOrDefault(x => x.Id == item.Key);
							if (tag == null) continue;

							if (set.Func == AggFunc.List)
							{
								response.Add(new HistoryResponse
								{
									Id = tag.Id,
									TagName = tag.Name,
									Type = tag.Type,
									Func = set.Func,
									Values = item
										.Select(x => new HistoryValue
										{
											Date = x.Date,
											Quality = x.Quality,
											Using = x.Using,
											Value = x.Value(),
										})
										.ToList(),
								});
							}
							else
							{
								var values = item
									.Where(x => x.Quality == TagQuality.Good || x.Quality == TagQuality.Good_ManualWrite)
									.Select(x => x.Value() as float?)
									.ToList();

								if (values.Count > 0)
								{
									float? value = 0;
									try
									{
										switch (set.Func)
										{
											case AggFunc.Sum: value = values.Sum(); break;
											case AggFunc.Avg: value = values.Average(); break;
											case AggFunc.Min: value = values.Min(); break;
											case AggFunc.Max: value = values.Max(); break;
										}
									}
									catch (Exception ex)
									{
										LogsWorker.Add("Api", "Aggregation " + set.Func + " on tag [\"" + tag.Name + "\"]: " + ex.Message, Workers.Logs.Models.LogType.Error);
									}

									response.Add(new HistoryResponse
									{
										Id = tag.Id,
										TagName = tag.Name,
										Type = tag.Type,
										Func = set.Func,
										Values = new List<HistoryValue>
										{
											new HistoryValue
											{
												Quality = TagQuality.Good,
												Using = TagHistoryUse.Aggregated,
												Value = value,
											}
										}
									});
								}
								else
								{
									response.Add(new HistoryResponse
									{
										Id = tag.Id,
										TagName = tag.Name,
										Type = tag.Type,
										Func = set.Func,
										Values = new List<HistoryValue>
										{
											new HistoryValue
											{
												Quality = TagQuality.Bad_NoValues,
												Using = TagHistoryUse.Aggregated,
												Value = 0,
											}
										}
									});
								}
							}
						}
					}
				}

				return response;
			}
		}

		public object Create(int sourceId = 0)
		{
			using (var db = new DatabaseContext())
			{
				var tag = new Tag { Name = "INSERTING" };
				var id = db.InsertWithInt32Identity(tag);

				tag.Name = "Tag_" + id;

				if (sourceId > 0)
				{
					var source = db.Sources.FirstOrDefault(x => x.Id == sourceId) ?? new Source { Name = "Unknown", Address = "", Id = sourceId };

					tag.SourceId = sourceId;
					tag.SourceItem = "";
					tag.Name = source.Name + "." + tag.Name;
				}

				db.Tags
					.Where(x => x.Id == id)
					.Set(x => x.Name, tag.Name)
					.Set(x => x.SourceId, tag.SourceId)
					.Set(x => x.SourceItem, tag.SourceItem)
					.Update();

				Cache.Write(new TagHistory
				{
					Date = DateTime.Now,
					Number = 0,
					Type = tag.Type,
					Quality = TagQuality.Bad,
					Text = string.Empty,
					TagId = id,
					Using = TagHistoryUse.Initial,
				});

				Cache.Update();

				return Done("Тег добавлен");
			}
		}

		public object CreateFromSource(int sourceId, string sourceItem, int sourceType)
		{
			using (var db = new DatabaseContext())
			{
				var source = db.Sources.FirstOrDefault(x => x.Id == sourceId);
				if (source == null)
				{
					return Error("Источник не найден");
				}

				string name = source.Name + "." + sourceItem;
				if (db.Tags.Any(x => x.Name == name))
				{
					return Error("Уже существует тег с таким именем");
				}

				var tag = new Tag
				{
					Name = name,
					Type = (TagType)sourceType,
					SourceId = sourceId,
					SourceItem = sourceItem,
				};

				var id = db.InsertWithInt32Identity(tag);

				Cache.Write(new TagHistory
				{
					Date = DateTime.Now,
					Number = 0,
					Type = tag.Type,
					Quality = TagQuality.Bad,
					Text = string.Empty,
					TagId = id,
					Using = TagHistoryUse.Initial,
				});

				Cache.Update();
			}

			return Done("Тег добавлен");
		}

		public object Read(int id)
		{
			using (var db = new DatabaseContext())
			{
				var tag = db.Tags.FirstOrDefault(x => x.Id == id);

				if (tag == null) return new { Error = "Тег не найден." };

				tag.Inputs = db.Rel_Tag_Input
					.Where(x => x.TagId == id)
					.ToList();

				return tag;
			}
		}

		public object Write(int id, object value = null, DateTime? date = null, bool good = true)
		{
			using (var db = new DatabaseContext())
			{
				var tag = db.Tags.FirstOrDefault(x => x.Id == id);
				if (tag == null)
				{
					return Error("Тег не найден по id [" + id + "]");
				}

				var d = date ?? DateTime.Now;
				var (text, number, q) = tag.FromRaw(value, (ushort)(good ? TagQuality.Good_ManualWrite : TagQuality.Bad_ManualWrite));

				db.WriteHistory(new List<TagHistory>
				{
					new TagHistory
					{
						Date = d,
						Text = text,
						Number = number,
						Quality = q,
						TagId = tag.Id,
						Type = tag.Type,
						Using = TagHistoryUse.Basic,
					}
				});

				return Done($"Значение {value} записано в {tag.Type} тег {tag.Name} с временем {d} и качеством {q}");
			}
		}

		public object Update(Tag tag)
		{
			using (var db = new DatabaseContext())
			{
				if (tag.Name.Contains(' ')) return Error("В имени тега не разрешены пробелы");
				if (tag.SourceItem.Contains(' ')) return Error("В адресе значения не разрешены пробелы");

				db.Tags
					.Where(x => x.Id == tag.Id)
					.Set(x => x.Name, tag.Name)
					.Set(x => x.Description, tag.Description)
					.Set(x => x.SourceId, tag.SourceId)
					.Set(x => x.SourceItem, tag.SourceItem)
					.Set(x => x.Interval, tag.Interval)
					.Set(x => x.Type, tag.Type)
					.Set(x => x.IsScaling, tag.IsScaling)
					.Set(x => x.MinRaw, tag.MinRaw)
					.Set(x => x.MaxRaw, tag.MaxRaw)
					.Set(x => x.MinEU, tag.MinEU)
					.Set(x => x.MaxEU, tag.MaxEU)
					.Set(x => x.IsCalculating, tag.IsCalculating)
					.Set(x => x.Formula, tag.Formula)
					.Update();

				db.Rel_Tag_Input
					.Where(x => x.TagId == tag.Id)
					.Delete();

				foreach (var input in tag.Inputs)
				{
					db.Rel_Tag_Input
						.Value(x => x.TagId, tag.Id)
						.Value(x => x.InputTagId, input.InputTagId)
						.Value(x => x.VariableName, input.VariableName)
						.Insert();
				}

				Cache.Update();

				return new { Done = "Тег успешно сохранён." };
			}
		}

		public object Delete(int id)
		{
			using (var db = new DatabaseContext())
			{
				db.Tags
					.Where(x => x.Id == id)
					.Delete();

				db.Rel_Tag_Input
					.Where(x => x.InputTagId == id || x.TagId == id)
					.Delete();

				db.Rel_Block_Tag
					.Where(x => x.TagId == id)
					.Delete();

				Cache.Update();

				return new { Done = "Тег успешно удалён." };
			}
		}
	}
}
