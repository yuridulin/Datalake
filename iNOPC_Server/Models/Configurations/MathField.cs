using System.Collections.Generic;

namespace iNOPC.Server.Models.Configurations
{
    public class MathField
    {
        public string Type { get; set; } = Types[0];

        public string Name { get; set; } = "Field1";

        public List<string> Fields { get; set; } = new List<string>();

        public float Value { get; set; } = 0;

        public float DefValue { get; set; } = 0;

        public static string[] Types => new[]
        {
            "SUM",
            "AVG",
            "DIFF",
            "MULT",
            "DIV",
            "CONST"
        };
    }
}