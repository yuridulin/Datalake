using Datalake.PrivateApi.Entities;
using Microsoft.AspNetCore.Http;

namespace Datalake.PrivateApi.Interfaces;

public interface IAuthenticator
{
	UserAccessEntity Authenticate(HttpContext httpContext);
}