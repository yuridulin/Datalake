using Datalake.Database.Repositories;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.Database.Tests.Scenarios
{
	public static class TagCrud
	{
		static string DbName = "tag";

		[Fact]
		public static async Task Process()
		{
			// setup
			using var db = await Setup.CreateDbContextAsync(DbName);
			var admin = await db.GetDefaultAdminAsync();
			Assert.NotNull(admin);

			string tagName = "TestTag";
			string newTagName = "TestTagUpdated";

			// create tag
			var request = new TagCreateRequest
			{
				Name = tagName,
				TagType = TagType.Number,
				SourceId = (int)SourceType.Manual,
				Frequency = TagFrequency.NotSet,
			};

			var createdTag = await TagsRepository.CreateAsync(db, admin, request);

			Assert.NotNull(createdTag);
			Assert.True(createdTag.Id > 0);

			// read tag
			var tagInfo = await GetTag(createdTag.Guid);

			Assert.NotNull(tagInfo);
			Assert.True(tagInfo.Name == tagName);
			Assert.True(tagInfo.SourceId == (int)SourceType.Manual);

			// update tag
			var updateRequest = new TagUpdateRequest
			{
				Name = newTagName,
				Frequency = TagFrequency.NotSet,
				Type = TagType.String,
			};
			await TagsRepository.UpdateAsync(db, admin, createdTag.Guid, updateRequest);
			var updatedTag = await GetTag(createdTag.Guid);

			Assert.NotNull(updatedTag);
			Assert.Equal(newTagName, updatedTag.Name);

			// delete tag
			await TagsRepository.DeleteAsync(db, admin, createdTag.Guid);
			var deletedTag = await GetTag(createdTag.Guid);

			Assert.Null(deletedTag);

			// clear
			await Setup.DisposeDatabaseAsync(DbName);

			async Task<TagInfo?> GetTag(Guid guid)
			{
				return await TagsRepository.ReadAsync(db, admin, guid);
			}
		}
	}
}
