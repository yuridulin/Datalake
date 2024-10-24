using Datalake.Database.Enums;
using Datalake.Database.Models.UserGroups;
using Datalake.Database.Models.Users;
using Datalake.Server.Constants;
using Datalake.Server.TestRunner.Attributes;
using Datalake.Server.TestRunner.Extensions;
using System.Net.Http.Json;

namespace Datalake.Server.TestRunner.Tests.CrudTests;

public class UserGroupCrud : IClassFixture<TestingWebAppFactory<Program>>
{
	private readonly TestingWebAppFactory<Program> _webAppFactory;
	private readonly HttpClient _httpClient;

	public UserGroupCrud(TestingWebAppFactory<Program> webAppFactory)
	{
		_webAppFactory = webAppFactory;
		_httpClient = _webAppFactory.CreateDefaultClient();
	}

	public async Task<string> Authorization()
	{
		var request = new UserLoginPass { Login = "admin", Password = "admin" };
		var response = await _httpClient.PostAsJsonAsync("api/users/auth", request);

		var token = response.Headers.GetValues(AuthConstants.TokenHeader).First();

		return token;
	}

	[Fact, Priority(1)]
	public async Task CreateReadUpdateDeleteComplete()
	{
		string controller = "api/userGroups";

		var token = await Authorization();
		_httpClient.DefaultRequestHeaders.Add(AuthConstants.TokenHeader, token);

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
