using iNOPC.Server.Web.RequestTypes;
using System;

namespace iNOPC.Server.Web.Api
{
	internal class StorageController : Controller
	{
		public object Read(DatalakeRequest req)
		{
			var tags = Storage.OPC.GetTagsByNames(req.Tags);

			return new
			{
				Timestamp = DateTime.Now,
				Tags = tags,
			};
		}
	}
}
