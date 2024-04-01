using System;
using System.Collections.Generic;

namespace DatalakeDatabase.Models;

public partial class RelTagInput
{
    public int TagId { get; set; }

    public int InputTagId { get; set; }

    public string? VariableName { get; set; }
}
