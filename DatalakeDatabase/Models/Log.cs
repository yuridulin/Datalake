using System;
using System.Collections.Generic;

namespace DatalakeDatabase.Models;

public partial class Log
{
    public DateTime Date { get; set; }

    public int Category { get; set; }

    public int? Ref { get; set; }

    public int Type { get; set; }

    public string Text { get; set; } = null!;

    public string? Details { get; set; }

    public string? User { get; set; }
}
