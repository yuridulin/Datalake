using System.Net;

namespace Datalake.Gateway.Host.Proxy.Abstractions;

/// <summary>
/// Исключение для прозрачного перенаправления ошибок
/// </summary>
public class ReverseProxyException(HttpStatusCode statusCode, string originalContent, string contentType)
	: Exception($"Proxy received error: {statusCode}")
{
	/// <summary>
	/// Код запроса
	/// </summary>
	public HttpStatusCode StatusCode { get; } = statusCode;

	/// <summary>
	/// Исходный контент
	/// </summary>
	public string OriginalContent { get; } = originalContent;

	/// <summary>
	/// Тип исходного контента
	/// </summary>
	public string ContentType { get; } = contentType;
}
