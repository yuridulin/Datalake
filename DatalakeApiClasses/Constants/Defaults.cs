using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.Users;

namespace DatalakeApiClasses.Constants;

public static class Defaults
{
	public static readonly UserCreateRequest InitialAdmin = new()
	{
		LoginName = "admin",
		Password = "admin",
		AccessType = AccessType.Admin,
		FullName = "Администратор",
	};
}
