using Datalake.Shared.Application.Entities;

namespace Datalake.Shared.Api.Interfaces;

public interface IAuthenticator
{
	UserAccessEntity Authenticate(Microsoft.AspNetCore.Http.HttpContext httpContext);
}