using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Server.Models
{
    public static class Maths
    {
        public static List<string> Fields { get; set; } = new List<string>();

        public static void Start()
        {
            Timer = new Timer(Program.Configuration.Settings.MathRecalculateMs);
            Timer.Elapsed += (s, e) => Calculate();

            Prepare();
            OPC.TagUpdated += (s, v) => IsActive = true;

            IsActive = true;
            Timer.Start();
            Task.Run(() => Calculate());
        }

        public static void Stop()
        {
            Timer.Stop();
            OPC.TagUpdated -= (s, v) => IsActive = true;
        }

        public static void Reset()
        {
            Prepare();
            Task.Run(() => Calculate(true));
        }


        static Timer Timer { get; set; }

        static bool IsActive { get; set; } = false;

        static void Prepare()
        {
            Fields.Clear();

            foreach (var mathfield in Program.Configuration.MathFields)
            {
                foreach (var field in mathfield.Fields)
                {
                    Fields.Add(field);
                }

                Fields.Add("Math." + mathfield.Name);
            }

            Fields = Fields.Distinct().ToList();

            foreach (var f in Fields)
            {
                Program.Log("Поле " + f);
            }

            lock (OPC.Tags)
            {
                foreach (var tag in OPC.Tags.Keys.ToArray())
                {
                    if (tag.StartsWith("Math."))
                    {
                        if (Program.Configuration.MathFields.Count(x => tag == "Math." + x.Name) == 0)
                        {
                            try
                            {
                                OPC.RemoveTag(OPC.Tags[tag]);
                                OPC.Tags.Remove(tag);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        static void Calculate(bool force = false)
        {
            //Program.Log("Событие расчёта мат. тегов. Флаги: " + IsActive);

            if (!force) if (!IsActive) return;

            lock (Program.Configuration.MathFields)
            {
                foreach (var mathfield in Program.Configuration.MathFields)
                {
                    var values = new List<float>();

                    foreach (var field in mathfield.Fields)
                    {
                        var _ = OPC.Read(field);
                        //Program.Log("Получение значения тега: " + field + " = " + _);

                        var v = float.TryParse(_.ToString(), out float f) ? f : 0;
                        values.Add(v);
                    }

                    //Program.Log("Расчёт тега " + mathfield.Name + " [" + mathfield.Type + "] = " + string.Join(" | ", mathfield.Fields.ToArray()));

                    try
                    {
                        switch (mathfield.Type)
                        {
                            case "SUM":
                                mathfield.Value = 0;
                                foreach (var v in values)
                                {
                                    mathfield.Value += v;
                                }
                                break;

                            case "AVG":
                                mathfield.Value = 0;
                                foreach (var v in values)
                                {
                                    mathfield.Value += v;
                                }
                                mathfield.Value /= values.Count;
                                break;

                            case "DIFF":
                                mathfield.Value = 0;
                                if (values.Count > 0)
                                {
                                    mathfield.Value = values[0];
                                }
                                for (int i = 1; i < values.Count; i++)
                                {
                                    mathfield.Value -= values[i];
                                }
                                break;

                            case "MULT":
                                mathfield.Value = 0;
                                if (values.Count > 0)
                                {
                                    mathfield.Value = values[0];
                                }
                                for (int i = 1; i < values.Count; i++)
                                {
                                    mathfield.Value *= values[i];
                                }
                                break;

                            case "DIV":
                                mathfield.Value = 0;
                                if (values.Count > 0)
                                {
                                    mathfield.Value = values[0];
                                }
                                for (int i = 1; i < values.Count; i++)
                                {
                                    mathfield.Value /= values[i];
                                }
                                break;

                            case "CONST":
                                mathfield.Value = mathfield.DefValue;
                                break;

                            default:
                                mathfield.Value = 0;
                                break;
                        }

                        //Program.Log("Расчёт тега " + mathfield.Name + " [" + mathfield.Type + "] (" + string.Join(" | ", mathfield.Fields.ToArray()) + ") = " + mathfield.Value);
                    }
                    catch (Exception e)
                    {
                        Program.Log("Ошибка в расчёте мат. тега \"" + mathfield.Name + "\": " + e.Message);
                    }

                    OPC.Write("Math." + mathfield.Name, mathfield.Value);
                }
            }

            IsActive = false;
        }
    }
}