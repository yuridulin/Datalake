using System;

namespace Datalake.Database
{
	public class Log : V0.Log
	{
		public Exception Exception
		{
			set
			{
				Details = value.Message + "\n" + value.StackTrace;
			}
		}
	}
}
