using iNOPC.Library;
using System;
using System.Collections.Generic;

namespace Energomera_CE102
{
	public class Driver : IDriver
	{
		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public Dictionary<string, DefField> Fields { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public event LogEvent LogEvent;
		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfiguration)
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		public void Write(string fieldName, object value)
		{
			throw new NotImplementedException();
		}
	}
}