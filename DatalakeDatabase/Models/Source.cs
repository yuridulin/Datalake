using System;
using System.Collections.Generic;

namespace DatalakeDatabase.Models;

public partial class Source
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int Type { get; set; }

    public string? Address { get; set; }
}
