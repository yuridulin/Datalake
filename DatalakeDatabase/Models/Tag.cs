using System;
using System.Collections.Generic;

namespace DatalakeDatabase.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Type { get; set; }

    public short Interval { get; set; }

    public int SourceId { get; set; }

    public string? SourceItem { get; set; }

    public bool IsScaling { get; set; }

    public float MinEu { get; set; }

    public float MaxEu { get; set; }

    public float MinRaw { get; set; }

    public float MaxRaw { get; set; }

    public bool IsCalculating { get; set; }

    public string? Formula { get; set; }
}
