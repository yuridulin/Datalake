using DatalakeDatabase.Enums;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Models;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;

namespace DatalakeDatabase;

public class DatalakeContext(DataOptions<DatalakeContext> options) : DataConnection(options.Options)
{
	public static void SetupLinqToDB()
	{
		Configuration.Linq.GuardGrouping = false;
	}

	/// <summary>
	/// Необходимые для работы записи, которые должны быть в базе данных
	/// </summary>
	public async Task EnsureDataCreatedAsync()
	{
		var customSources = Enum.GetValues<CustomSource>()
			.Select(x => new Source
			{
				Id = (int)x,
				Name = x.ToString(),
				Description = x.GetDescription(),
				Type = SourceType.Custom,
			})
			.ToArray();

		foreach (var source in customSources)
		{
			if (!await Sources.AnyAsync(x => x.Id == source.Id))
			{
				int count = await Sources
					.Value(x => x.Id, source.Id)
					.Value(x => x.Name, source.Name)
					.Value(x => x.Description, source.Description)
					.Value(x => x.Type, source.Type)
					.InsertAsync();

				if (count == 0)
					throw new Exception("Не удалось добавить источник по умолчанию: " + (CustomSource)source.Id);
			}
		}

		if (!await Settings.AnyAsync())
		{
			int count = await Settings
				.Value(x => x.LastUpdate, DateTime.Now)
				.InsertAsync();

			if (count == 0)
				throw new Exception("Не удалось создать строку настроек");
		}
	}

	#region Таблицы

	public ITable<Block> Blocks
		=> this.GetTable<Block>();

	public ITable<BlockProperty> BlockProperties
		=> this.GetTable<BlockProperty>();

	public ITable<BlockTag> BlockTags
		=> this.GetTable<BlockTag>();

	public ITable<Settings> Settings
		=> this.GetTable<Settings>();

	public ITable<Source> Sources
		=> this.GetTable<Source>();

	public ITable<Tag> Tags
		=> this.GetTable<Tag>();

	public ITable<TagHistoryChunk> TagHistoryChunks
		=> this.GetTable<TagHistoryChunk>();

	public ITable<TagInput> TagInputs
		=> this.GetTable<TagInput>();

	public ITable<TagHistory> TagsLive
		=> this.GetTable<TagHistory>();

	public ITable<User> Users
		=> this.GetTable<User>();

	#endregion
}
