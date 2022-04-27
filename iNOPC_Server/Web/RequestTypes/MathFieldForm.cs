using System.Collections.Generic;

namespace iNOPC.Server.Web.RequestTypes
{
    public class MathFieldForm
    {
        public string Name { get; set; } = "";

        public string OldName { get; set; } = "";

        public string Type { get; set; } = "SUM";

        public float DefValue { get; set; } = 0;

        public List<string> Fields { get; set; } = new List<string>();
    }
}