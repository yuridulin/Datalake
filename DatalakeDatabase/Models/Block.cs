using System;
using System.Collections.Generic;

namespace DatalakeDatabase.Models;

public partial class Block
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
