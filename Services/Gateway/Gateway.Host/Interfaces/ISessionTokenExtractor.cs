namespace Datalake.Gateway.Host.Interfaces;

public interface ISessionTokenExtractor
{
	string ExtractToken(HttpRequest request);

	string ExtractToken(HttpContext context);
}
