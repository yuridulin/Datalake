using iNOPC.Library;
using iNOPC.Server.Database.Models;
using iNOPC.Server.Models;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace iNOPC.Server.Database
{
    public static class Storage
    {
        static List<Point> Queue { get; set; } = new List<Point>();

        static Timer T { get; set; }

        public static void Start()
        {
            if (!Program.Configuration.Database.UseDatabase) return;

            T = new Timer(Program.Configuration.Database.StoreIntervalS * 1000);
            T.Elapsed += (s, e) =>
            {
                Point[] points;

                lock (Queue)
                {
                    points = Queue.ToArray();
                    Queue.Clear();
                }

                foreach (var point in points)
                {
                    Console.WriteLine(point.TimeStamp + " " + point.ValueString);
                }

                using (var db = new DatabaseContext())
                {
                    foreach (var point in points)
                    {
                        db.Insert(point);
                    }
                }

            };
            T.Start();
        }

        public static void Stop()
        {
            try { T?.Stop(); } catch { }
            try { T = null; } catch { }
        }

        public static void Add(Device device, Dictionary<string, DefField> fields)
        {
            lock (Queue)
            {
                foreach (var field in fields)
                {
                    Queue.Add(new Point
                    {
                        TimeStamp = DateTime.Now,
                        Driver = device.DriverName,
                        Device = device.Name,
                        Field = field.Key,
                        Quality = field.Value.Quality,
                        Value = field.Value.Value,
                        ValueString = field.Value.Value?.ToString() ?? null,
                        ValueType = field.Value.Value?.GetType()?.ToString() ?? "String",
                    });
                }
            }
        }

        public static List<DefField> Get(string deviceName, string driverName, DateTime start, DateTime end, bool isChangedOnly)
        {
            using (var db = new DatabaseContext())
            {
                var fields = db.Points
                    .Where(x => x.Driver == driverName && x.Device == deviceName)
                    .Where(x => x.TimeStamp >= start && x.TimeStamp <= end)
                    .Select(x => new DefField
                    {
                        Name = x.Field,
                        Quality = (ushort)x.Quality,
                        Value = x.ValueType == "System.Int32" ? (object)int.Parse(x.ValueString) 
                            : x.ValueString == "System.Boolean" ? (object)bool.Parse(x.ValueString)
                            : x.ValueString == "System.String" ? (object)x.ValueString 
                            : null,
                    })
                    .ToList();

                return fields;
            }
        }
    }
}