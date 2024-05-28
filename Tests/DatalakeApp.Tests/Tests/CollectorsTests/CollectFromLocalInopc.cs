using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.Sources;
using DatalakeApiClasses.Models.Tags;
using DatalakeApiClasses.Models.Values;
using DatalakeServer.Services.Receiver.Models.Inopc;
using DatalakeServer.Tests.Attributes;
using DatalakeServer.Tests.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DatalakeServer.Tests.Tests.CollectorsTests;

public class CollectFromLocalInopc : IClassFixture<TestingWebAppFactory<Program>>
{
	private readonly TestingWebAppFactory<Program> _webAppFactory;
	private readonly ILogger<CollectFromLocalInopc> _logger;
	private readonly HttpClient _httpClient;

	public CollectFromLocalInopc(TestingWebAppFactory<Program> webAppFactory)
	{
		_webAppFactory = webAppFactory;
		_logger = new LoggerFactory().CreateLogger<CollectFromLocalInopc>();
		_httpClient = _webAppFactory.CreateDefaultClient();
	}


	const string LocalInopcIp = "10.208.4.113";
	const string LocalInopcUrl = $"http://{LocalInopcIp}:81/api/storage/read";
	const string TestSourceName = "LocalInopcTest";
	const string TestItemPath = "Test.TestDevice.Num1";
	const string TestTagName = "TestTag1";

	[Fact, Priority(0)]
	public void T000_Prepare()
	{
		Assert.True(true);
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
		var sources = await _httpClient.GetAsync<SourceInfo[]>("api/sources");
		Assert.NotNull(sources);

		var testSource = sources.FirstOrDefault(x => x.Name == TestSourceName);
		Assert.NotNull(testSource);

		var items = await _httpClient.GetAsync<SourceEntryInfo[]>($"api/sources/{testSource.Id}/tags");
		Assert.True(items.Length >= 3);
		Assert.Contains(items, x => x.ItemInfo?.Path == TestItemPath);
	}

	[Fact, Priority(4)]
	public async Task T004_CreateTestTagForTestSource()
	{
		var sources = await _httpClient.GetAsync<SourceInfo[]>("api/sources");
		Assert.NotNull(sources);

		var testSource = sources.FirstOrDefault(x => x.Name == TestSourceName);
		Assert.NotNull(testSource);

		var id = await _httpClient.PostAsync<int>("api/tags", new TagCreateRequest
		{
			Name = TestTagName,
			TagType = TagType.Number,
			SourceId = testSource.Id,
			SourceItem = TestItemPath,
		});

		Assert.True(id > 0);
	}

	[Fact, Priority(5)]
	public async Task T005_EnsureValueOfTestTagsUpdatesTwice()
	{
		await Task.Delay(TimeSpan.FromSeconds(1));

		object? storedValue = null;

		for (var i = 0; i < 4; i++)
		{
			var request = new ValuesRequest[]
			{
				new()
				{
					TagNames = [TestTagName]
				}
			};

			var responses = await _httpClient.PostAsync<List<ValuesResponse>>("api/tags/values", request);
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
		var sources = await _httpClient.GetAsync<SourceInfo[]>("api/sources");
		Assert.NotNull(sources);

		var testSource = sources.FirstOrDefault(x => x.Name == TestSourceName);
		Assert.NotNull(testSource);

		await _httpClient.DeleteAsync("api/sources/" + testSource.Id);

		var tags = await _httpClient.GetAsync<TagInfo[]>("api/tags");
		Assert.NotNull(tags);

		var testTag = tags.FirstOrDefault(x => x.Name == TestTagName);
		Assert.NotNull(testTag);

		await _httpClient.DeleteAsync("api/tags/" + testTag.Id);
	}
}
