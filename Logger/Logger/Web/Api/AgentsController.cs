using Logger.Web.Models;
using Logger_Library;
using System;
using System.Collections.Generic;

namespace Logger.Web.Api
{
	public class AgentsController : Controller
	{
		public object Install(string endpoint)
		{
			return new { Error = "Not implemented" };
		}

		public object Uninstall(string endpoint)
		{
			return new { Error = "Not implemented" };
		}

		public object Reply(List<Log> logs)
		{
			foreach (var log in logs)
			{
				Console.WriteLine(log.ToConsole());
			}

			return new { Done = true };
		}
	}
}
