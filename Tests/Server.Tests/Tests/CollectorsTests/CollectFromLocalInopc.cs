using Datalake.Database.Enums;
using Datalake.Database.Models.Sources;
using Datalake.Database.Models.Tags;
using Datalake.Database.Models.Users;
using Datalake.Database.Models.Values;
using Datalake.Server.Constants;
using Datalake.Server.Services.Receiver.Models.Inopc;
using Datalake.Server.TestRunner.Attributes;
using Datalake.Server.TestRunner.Extensions;
using System.Net.Http.Json;

namespace Datalake.Server.Tests.Tests.CollectorsTests;

public class CollectFromLocalInopc(TestingWebAppFactory<Program> webAppFactory)
	: IClassFixture<TestingWebAppFactory<Program>>
{
	private readonly HttpClient _httpClient = webAppFactory.CreateDefaultClient();


	const string LocalInopcIp = "10.208.4.113";
	const string LocalInopcUrl = $"http://{LocalInopcIp}:81/api/storage/read";
	const string TestSourceName = "LocalInopcTest";
	const string TestItemPath = "Test.TestDevice.Num1";
	const string TestTagName = "TestTag1";

	private async Task<string> Authorization()
	{
		var request = new UserLoginPass { Login = "admin", Password = "admin" };
		var response = await _httpClient.PostAsJsonAsync("api/users/auth", request);

		var token = response.Headers.GetValues(AuthConstants.TokenHeader).First();

		return token;
	}

	[Fact, Priority(0)]
	public async Task T000_PrepareAsync()
	{
		Assert.True(true);

		var token = await Authorization();

		Assert.NotNull(token);
	}

	[Fact, Priority(1)]
	public async Task T001_LocalInopcMustBeAvailable()
	{
		var request = new InopcRequest
		{
			Tags = []
		};

		var globalHttpClient = new HttpClient();
		var response = await globalHttpClient.PostAsync(LocalInopcUrl, JsonContent.Create(request));

		Assert.True(response.IsSuccessStatusCode);
	}

	[Fact, Priority(2)]
	public async Task T002_CreateSourceForLocalInopc()
	{
		var token = await Authorization();
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, token);

		var body = new SourceInfo
		{
			Name = TestSourceName,
			Type = SourceType.Inopc,
			Address = LocalInopcIp,
		};

		int sourceId = await _httpClient.PostAsync<int>("api/sources", body);
		Assert.True(sourceId > 0);
	}

	[Fact, Priority(3)]
	public async Task T003_GetItemsListForTestSource()
	{
		var token = await Authorization();
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, token);

		var sources = await _httpClient.GetAsync<SourceInfo[]>("api/sources");
		Assert.NotNull(sources);

		var testSource = sources.FirstOrDefault(x => x.Name == TestSourceName);
		Assert.NotNull(testSource);

		var items = await _httpClient.GetAsync<SourceEntryInfo[]>($"api/sources/{testSource.Id}/items-and-tags");
		Assert.True(items.Length >= 3);
		Assert.Contains(items, x => x.ItemInfo?.Path == TestItemPath);
	}

	[Fact, Priority(4)]
	public async Task T004_CreateTestTagForTestSource()
	{
		var token = await Authorization();
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, token);

		var sources = await _httpClient.GetAsync<SourceInfo[]>("api/sources");
		Assert.NotNull(sources);

		var testSource = sources.FirstOrDefault(x => x.Name == TestSourceName);
		Assert.NotNull(testSource);

		var guid = await _httpClient.PostAsync<Guid>("api/tags", new TagCreateRequest
		{
			Name = TestTagName,
			TagType = TagType.Number,
			SourceId = testSource.Id,
			SourceItem = TestItemPath,
		});

		await _httpClient.PutAsync("api/tags/" + guid, new TagUpdateRequest
		{
			Name = TestTagName,
			IntervalInSeconds = 0,
			Type = TagType.Number,
			SourceId = testSource.Id,
			SourceItem = TestItemPath,
			IsScaling = false,
		});
	}

	[Fact, Priority(5)]
	public async Task T005_EnsureValueOfTestTagsUpdatesTwice()
	{
		await Task.Delay(TimeSpan.FromSeconds(3));

		var token = await Authorization();
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, token);

		object? storedValue = null;

		var tags = await _httpClient.GetAsync<TagInfo[]>("api/tags");
		var tag = tags.FirstOrDefault(x => x.Name == TestTagName);
		Assert.NotNull(tag);

		for (var i = 0; i < 4; i++)
		{
			var request = new ValuesRequest[]
			{
				new()
				{
					RequestKey = "1",
					Tags = [tag.Guid]
				}
			};

			var responses = await _httpClient.PostAsync<List<ValuesTagResponse>>("api/tags/values", request);
			Assert.NotNull(responses);
			Assert.Single(responses);

			var currentValues = responses[0].Values;
			Assert.NotNull(currentValues);
			Assert.Single(currentValues);

			var currentValue = currentValues[0];
			Assert.NotNull(currentValue);
			Assert.False(currentValue.Value == storedValue);

			storedValue = currentValue.Value;
			await Task.Delay(TimeSpan.FromSeconds(1));
		}
	}

	[Fact, Priority(int.MaxValue)]
	public async Task T999_RemoveTestObjects()
	{
		var token = await Authorization();
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, token);

		var sources = await _httpClient.GetAsync<SourceInfo[]>("api/sources");
		Assert.NotNull(sources);

		var testSource = sources.FirstOrDefault(x => x.Name == TestSourceName);
		if (testSource != null)
		{
			await _httpClient.DeleteAsync("api/sources/" + testSource.Id);
		}

		var tags = await _httpClient.GetAsync<TagInfo[]>("api/tags");
		Assert.NotNull(tags);

		var testTag = tags.FirstOrDefault(x => x.Name == TestTagName);
		if (testTag != null)
		{
			Assert.NotNull(testTag);
			await _httpClient.DeleteAsync("api/tags/" + testTag.Guid);
		}
	}
}
