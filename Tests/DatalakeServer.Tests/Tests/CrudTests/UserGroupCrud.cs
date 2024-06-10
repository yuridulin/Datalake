using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.UserGroups;
using DatalakeApiClasses.Models.Users;
using DatalakeServer.Constants;
using DatalakeServer.TestRunner.Attributes;
using DatalakeServer.TestRunner.Extensions;
using System.Net.Http.Json;

namespace DatalakeServer.TestRunner.Tests.CrudTests;

public class UserGroupCrud : IClassFixture<TestingWebAppFactory<Program>>
{
	private readonly TestingWebAppFactory<Program> _webAppFactory;
	private readonly HttpClient _httpClient;

	public UserGroupCrud(TestingWebAppFactory<Program> webAppFactory)
	{
		_webAppFactory = webAppFactory;
		_httpClient = _webAppFactory.CreateDefaultClient();
	}

	public async Task<(string, string)> Authorization()
	{
		var request = new UserLoginPass { Login = "admin", Password = "admin" };
		var response = await _httpClient.PostAsJsonAsync("api/users/auth", request);

		var token = response.Headers.GetValues(AuthConstants.TokenHeader).First();
		var name = response.Headers.GetValues(AuthConstants.NameHeader).First();

		return (token, name);
	}

	[Fact, Priority(1)]
	public async Task CreateReadUpdateDeleteComplete()
	{
		string controller = "api/userGroups";

		var (token, name) = await Authorization();
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, token);
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.NameHeader, name);

		string userGroupName = "TestUserGroup";

		var userGroupGuid = await _httpClient.PostAsync<string>(controller, new UserGroupCreateRequest
		{
			Name = userGroupName,
		});
		Assert.True(Guid.TryParse(userGroupGuid, out var _));

		var userGroup = await _httpClient.GetAsync<UserGroupInfo>(controller + "/" + userGroupGuid);
		Assert.NotNull(userGroup);

		await _httpClient.PutAsync(controller + "/" + userGroupGuid, new UserGroupUpdateRequest
		{
			Name = userGroupName,
			Users = [],
			Groups = [],
			AccessType = AccessType.Admin,
			Description = "test",
		});
		var userGroupAfterUpdate = await _httpClient.GetAsync<UserGroupInfo>(controller + "/" + userGroupGuid);
		Assert.NotNull(userGroup);
		Assert.Equal("test", userGroupAfterUpdate.Description);

		await _httpClient.DeleteAsync(controller + "/" + userGroupGuid);
		var userGroups = await _httpClient.GetAsync<UserGroupInfo[]>(controller);
		Assert.NotNull(userGroups);
		Assert.DoesNotContain(userGroupName, userGroups.Select(x => x.Name));
	}
}
