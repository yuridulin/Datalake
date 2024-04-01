using System;
using System.Collections.Generic;

namespace DatalakeDatabase.Models;

public partial class Setting
{
    public string Key { get; set; } = null!;

    public string? Value { get; set; }
}
