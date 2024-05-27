using DatalakeApiClasses.Models.Abstractions;
using DatalakeApiClasses.Models.Users;

namespace DatalakeDatabase.Extensions;

public static class UserRightsExtension
{
	public static IRights Merge(this IEnumerable<IRights> rightsArray)
	{
		if (!rightsArray.Any())
			return new UserRights();

		var rights = new UserRights
		{
			UserGuid = rightsArray.Select(x => x.UserGuid).FirstOrDefault(),

			HasAccessToTag = rightsArray.Any(x => x.HasAccessToTag == true),
			CanManageTag = rightsArray.Any(x => x.CanManageTag == true),
			CanWriteToTag = rightsArray.Any(x => x.CanWriteToTag == true),

			HasAccessToSource = rightsArray.Any(x => x.HasAccessToSource == true),
			CanManageSource = rightsArray.Any(x => x.CanManageSource == true),
			CanManageSourceTags = rightsArray.Any(x => x.CanManageSourceTags == true),

			HasAccessToBlock = rightsArray.Any(x => x.HasAccessToBlock == true),
			CanManageBlock = rightsArray.Any(x => x.CanManageBlock == true),
			CanManageBlockTags = rightsArray.Any(x => x.CanManageBlockTags == true),

			CanControlAccess = rightsArray.Any(x => x.CanControlAccess == true),
			CanViewLogs = rightsArray.Any(x => x.CanViewLogs == true),
			CanViewSystemTags = rightsArray.Any(x => x.CanViewSystemTags == true),
		};

		return rights;
	}
}
