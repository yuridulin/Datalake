using System;
using System.Collections.Generic;

namespace DatalakeDatabase.Models;

public partial class RelBlockTag
{
    public int BlockId { get; set; }

    public int TagId { get; set; }

    public string? Name { get; set; }

    public int Type { get; set; }
}
