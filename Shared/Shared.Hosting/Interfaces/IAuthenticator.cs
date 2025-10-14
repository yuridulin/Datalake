using Datalake.Domain.ValueObjects;

namespace Datalake.Shared.Hosting.Interfaces;

public interface IAuthenticator
{
	UserAccessValue Authenticate(Microsoft.AspNetCore.Http.HttpContext httpContext);
}