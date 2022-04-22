using System;

namespace iNOPC.Server.Web.RequestTypes
{
    public class History
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public long Id { get; set; }

        public bool IsChangeOnly { get; set; }
    }
}