namespace TaskManager;

public enum TaskState
{
    Running,
    Paused,
    Completed
}

public class TaskLog
{
    public int Id { get; set; }
    public UserTask Task { get; set; } = new();

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public TaskState State { get; set; }

    public TimeSpan TotalActiveTime { get; set; } = TimeSpan.Zero;
    public DateTime? LastResumedAt { get; set; }

    
    public TimeSpan Duration
    {
        get
        {
            if (State == TaskState.Running && LastResumedAt != null)
            {
                return TotalActiveTime + (DateTime.Now - LastResumedAt.Value);
            }

            return TotalActiveTime;
        }
    }
}