using System;
using System.Collections.Generic;

namespace ABK.Models;

public partial class Log
{
    public int Id { get; set; }

    public string Action { get; set; } = null!;

    public int? TaskId { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ToDoTask? Task { get; set; }

    public virtual User? User { get; set; }
}
