using LinqToDB.Mapping;
using System;

namespace iNOPC.Server.Database.Models
{
    [Table(Name = "Points")]
    public class Point
    {
        [Column]
        public string Driver { get; set; }

        [Column]
        public string Device { get; set; }

        [Column]
        public string Field { get; set; }

        [Column]
        public string ValueType { get; set; }

        [Column]
        public string ValueString { get; set; }

        [Column]
        public DateTime TimeStamp { get; set; }

        [Column]
        public int Quality { get; set; }

        // поля

        public object Value { get; set; }
    }
}