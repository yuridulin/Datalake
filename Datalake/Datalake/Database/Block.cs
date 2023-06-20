using Datalake.Database.Enums;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Datalake.Database
{
	[Table(Name = "Blocks")]
	public class Block
	{
		[Column, PrimaryKey, Identity]
		public int Id { get; set; } = 0;

		[Column, NotNull]
		public int ParentId { get; set; } = 0;

		[Column, NotNull]
		public string Name { get; set; } = string.Empty;

		[Column]
		public string Description { get; set; } = string.Empty;

		[Column]
		public string PropertiesRaw { get; set; } = string.Empty;


		// поля для маппинга

		public Dictionary<string, string> Properties 
		{
			get
			{
				return JsonConvert.DeserializeObject<Dictionary<string, string>>(PropertiesRaw);
			}
			set
			{
				PropertiesRaw = JsonConvert.SerializeObject(value);
			}
		}

		public Dictionary<BlockTagType, List<Tag>> Tags { get; set; } = new Dictionary<BlockTagType, List<Tag>>();

		public List<Block> Children { get; set; } = new List<Block>();
	}
}
