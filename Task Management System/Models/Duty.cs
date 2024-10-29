using System;
using System.Collections.Generic;

namespace Task_Management_System.Models;

public partial class Duty
{
    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime DueDate { get; set; }

    public string Priority { get; set; } = null!;

    public string Category { get; set; } = null!;

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string UserId { get; set; } = null!;

}
