using Datalake.Domain.ValueObjects;

namespace Datalake.Shared.Hosting.Interfaces;

/// <summary>
/// Служба, позволяющая аутентифицировать пользователя на основе переданных с запросом данных
/// </summary>
public interface IAuthenticator
{
	/// <summary>
	/// Аутентификация пользователя на основе переданных с запросом данных
	/// </summary>
	UserAccessValue Authenticate(Microsoft.AspNetCore.Http.HttpContext httpContext);
}