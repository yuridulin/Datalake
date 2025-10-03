using Datalake.Shared.Application.Entities;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Сервис аутентификации пользователей по входным данным
/// </summary>
public interface IAuthenticatorService
{
	/// <summary>
	/// Аутентификация пользователя по сессионному токену из запроса
	/// </summary>
	/// <returns>Данные о пользователе и его правах</returns>
	/// <exception cref="ForbiddenException">Сессия не открыта</exception>
	/// <exception cref="NotFoundException">Сессия не найдена</exception>
	UserAccessEntity Authenticate(HttpContext httpContext);
}
