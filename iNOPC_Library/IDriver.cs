using System.Collections.Generic;

namespace iNOPC.Library
{
	public interface IDriver
    {
        event LogEvent LogEvent;

        event UpdateEvent UpdateEvent;

        Dictionary<string, DefField> Fields { get; set; }

        string Version { get; }

        bool Start(string jsonConfiguration);

        void Stop();

        void Write(string fieldName, object value);
    }
}