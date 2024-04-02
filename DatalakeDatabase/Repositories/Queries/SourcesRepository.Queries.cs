using DatalakeDatabase.ApiModels.Sources;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatalakeDatabase.Repositories
{
	public partial class SourcesRepository
	{
		public async Task<SourceTagInfo[]> GetExistTagsAsync(int id)
		{
			return await db.Tags
				.Where(x => x.SourceId == id)
				.Select(x => new SourceTagInfo
				{
					Id = x.Id,
					Name = x.Name,
					Type = x.Type,
				})
				.ToArrayAsync();
		}
	}
}
