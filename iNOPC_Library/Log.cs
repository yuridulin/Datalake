using System;

namespace iNOPC.Library
{
    public struct Log
    {
        public uint Id { get; set; }

        public DateTime Date { get; set; }

        public string Text { get; set; }

        public LogType Type { get; set; }
    }
}