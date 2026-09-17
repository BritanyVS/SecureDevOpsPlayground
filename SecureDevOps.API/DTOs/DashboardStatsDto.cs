namespace SecureDevOps.API.DTOs;

public class DashboardStatsDto
{
    public int TotalTasks { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int LowPriority { get; set; }
    public int MediumPriority { get; set; }
    public int HighPriority { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByPriority { get; set; } = new();
    public List<TopAssigneeDto> TopAssignees { get; set; } = new();
    public List<TaskItem.TaskItemResponseDto> RecentTasks { get; set; } = new();
}

public class TopAssigneeDto
{
    public string Username { get; set; } = string.Empty;
    public int Count { get; set; }
}