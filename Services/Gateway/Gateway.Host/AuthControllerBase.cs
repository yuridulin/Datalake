using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с пользователями
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class AuthControllerBase : ControllerBase
{
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "auth";

	/// <inheritdoc cref="AuthenticateLocalAsync(UserLoginPass)" />
	public const string AuthenticateLocal = "local";

	/// <inheritdoc cref="AuthenticateEnergoIdUser" />
	public const string AuthenticateEnergoIdUser = "energo-id";

	/// <inheritdoc cref="IdentifyAsync" />
	public const string Identify = "identify";

	/// <inheritdoc cref="LogoutAsync" />
	public const string Logout = "logout";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Аутентификация локального пользователя по связке "имя для входа/пароль"
	/// </summary>
	/// <param name="loginPass">Данные для входа</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost(AuthenticateLocal)]
	public abstract Task<ActionResult<UserSessionInfo>> AuthenticateLocalAsync(
		[BindRequired, FromBody] UserLoginPass loginPass);

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Аутентификация пользователя, прошедшего проверку на сервере EnergoId
	/// </summary>
	/// <param name="energoIdInfo">Данные пользователя Keycloak</param>
	/// <returns>Данные о учетной записи</returns>
	[HttpPost(AuthenticateEnergoIdUser)]
	public abstract Task<ActionResult<UserSessionInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] UserEnergoIdInfo energoIdInfo);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о учетной записи на основе текущей сессии
	/// </summary>
	/// <returns>Данные о учетной записи</returns>
	[HttpGet(Identify)]
	public abstract Task<ActionResult<UserSessionInfo?>> IdentifyAsync();

	/// <summary>
	/// <see cref="HttpMethod.Delete"/>: Закрытие уканной сессии пользователя
	/// </summary>
	/// <param name="token">Сессионный токен доступа</param>
	[HttpDelete(Logout)]
	public abstract Task<ActionResult> LogoutAsync(
		[BindRequired, FromQuery] string token);

	#endregion Методы
}