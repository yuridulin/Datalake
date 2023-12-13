using System;

namespace Datalake.Database
{
	public class Log : V2.Log
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
