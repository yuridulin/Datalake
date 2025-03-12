using Datalake.PublicApi.Exceptions;

namespace Datalake.Database.Constants;

internal static class Errors
{
	internal static ForbiddenException NoAccess { get; } = new(message: "нет доступа");
}
