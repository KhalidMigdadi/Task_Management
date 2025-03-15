using System;
using System.Collections.Generic;

namespace ABK.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? Points { get; set; }

    public string? Image { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<ToDoTask> ToDoTasks { get; set; } = new List<ToDoTask>();
}
