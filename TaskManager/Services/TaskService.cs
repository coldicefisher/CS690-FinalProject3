using System.Linq; // For Any() and FirstOrDefault()

namespace TaskManager;

public class TaskService
{
    private readonly StorageService _storage;
    private List<TaskLog> _logs;

    public TaskLog? CurrentTask { get; private set; }

    private List<Category> _categories;

    public IReadOnlyList<Category> Categories => _categories;

    // We need to use dependency injection for the storage service to allow for testing with a fake storage service
    //  that doesn't touch the file system. In production, we'll use the real storage service.

    // We use constructor chaining to provide a default constructor that uses the real storage service, 
    // while still allowing for a custom storage service to be injected for testing purposes.
    public TaskService() : this(new StorageService())
    {
    }

    public TaskService(StorageService storage)
    {
        _storage = storage;
        _logs = _storage.Load();
        _categories = _storage.LoadCategories();

        // Seed default categories if none exist
        if (_categories.Count == 0)
        {
            _categories.Add(new Category { Id = 1, Name = "Work" });
            _categories.Add(new Category { Id = 2, Name = "Personal" });
            _categories.Add(new Category { Id = 3, Name = "Study" });

            _storage.SaveCategories(_categories);
        }
    }

    public bool StartTask(string name, int categoryId)
    {
        if (CurrentTask != null)
            return false;

        var category = _categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
            return false;

        CurrentTask = new TaskLog
        {
            Id = _logs.Any() ? _logs.Max(l => l.Id) + 1 : 1,
            Task = new UserTask
            {
                Name = name,
                CategoryId = category.Id,
                Category = category
            },
            StartTime = DateTime.Now,
            LastResumedAt = DateTime.Now,
            State = TaskState.Running
        };

        return true;
    }

    public void PauseTask()
    {
        if (CurrentTask == null || CurrentTask.State != TaskState.Running)
            return;

        var elapsed = DateTime.Now - CurrentTask.LastResumedAt!.Value;
        CurrentTask.TotalActiveTime += elapsed;

        CurrentTask.LastResumedAt = null;
        CurrentTask.State = TaskState.Paused;
    }

    public void ResumeTask()
    {
        if (CurrentTask == null || CurrentTask.State != TaskState.Paused)
            return;

        CurrentTask.LastResumedAt = DateTime.Now;
        CurrentTask.State = TaskState.Running;
    }

    public void CompleteTask()
    {
        if (CurrentTask == null)
            return;

        if (CurrentTask.State == TaskState.Running)
        {
            var elapsed = DateTime.Now - CurrentTask.LastResumedAt!.Value;
            CurrentTask.TotalActiveTime += elapsed;
        }

        CurrentTask.EndTime = DateTime.Now;
        CurrentTask.State = TaskState.Completed;

        _logs.Add(CurrentTask);
        _storage.Save(_logs);

        CurrentTask = null;
    }

    public void DiscardTask()
    {
        CurrentTask = null;
    }

    public List<TaskLog> GetTodayTasks()
    {
        return _logs
            .Where(t => t.StartTime.Date == DateTime.Today)
            .ToList();
    }

    public Category AddCategory(string name)
    {
        var newCategory = new Category
        {
            Id = _categories.Any() ? _categories.Max(c => c.Id) + 1 : 1,
            Name = name
        };

        _categories.Add(newCategory);
        _storage.SaveCategories(_categories);

        return newCategory;
    }


    // We leverage Lynq's grouping capabilities to chain together the grouping and sorting logic.
    public List<IGrouping<WeekWindow, TaskLog>> GetWeeklyGroups() 
    {
        return _logs
            .GroupBy(log =>
            {
                var diff = (7 + (log.StartTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weekStart = log.StartTime.Date.AddDays(-diff);

                return new WeekWindow(weekStart);
            })
            .OrderByDescending(g => g.Key.Start)
            .ToList();
    }

    public void DeleteTaskLog(int id)
    {
        var log = _logs.FirstOrDefault(l => l.Id == id);
        if (log == null)
            return;

        _logs.Remove(log);
        _storage.Save(_logs);
    }
    
}