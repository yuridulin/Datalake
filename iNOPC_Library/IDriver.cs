using System.Collections.Generic;

namespace iNOPC.Library
{
	public interface IDriver
    {
        event LogEvent LogEvent;

        event UpdateEvent UpdateEvent;

        event WinLogEvent WinLogEvent;

        Dictionary<string, DefField> Fields { get; set; }

        bool Start(string jsonConfiguration);

        void Stop();

        void Write(string fieldName, object value);
    }
}