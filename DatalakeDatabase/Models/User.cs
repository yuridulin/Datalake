using System;
using System.Collections.Generic;

namespace DatalakeDatabase.Models;

public partial class User
{
    public string Name { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public int AccessType { get; set; }

    public string FullName { get; set; } = null!;

    public string? StaticHost { get; set; }
}
