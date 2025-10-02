using Datalake.Shared.Domain.Entities;

namespace Datalake.Shared.Api.Interfaces;

public interface IAuthenticator
{
	UserAccessEntity Authenticate(Microsoft.AspNetCore.Http.HttpContext httpContext);
}