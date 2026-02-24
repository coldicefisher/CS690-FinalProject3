namespace TaskManager;

using Spectre.Console;

public class TaskManagerUI
{
    private readonly TaskService _service;
    private bool _running = true;

    public TaskManagerUI(TaskService service)
    {
        _service = service;
    }

    public void Run()
    {
        while (_running)
        {
            if (_service.CurrentTask != null)
            {
                ShowWorkingMenu();
                continue;
            }

            ShowMainMenu();
        }
    }

    private void ShowMainMenu()
    {
        Console.WriteLine(new string('*', 60));
        Console.WriteLine("Task Manager");
        Console.WriteLine(new string('*', 60));
        Console.WriteLine();
        Console.WriteLine("1. Start Task");
        Console.WriteLine("2. View Daily Summary");
        Console.WriteLine("3. View Weekly Summary");
        Console.WriteLine("4. Exit");
        Console.Write("Select option: ");

        var input = Console.ReadLine();

        switch (input)
        {
            case "1":
                StartTask();
                break;

            case "2":
                ShowDailySummary();
                break;

            case "3":
                ShowWeeklySummary();
                break;

            case "4":
                _running = false;
                break;
        }
    }

    private void StartTask()
    {
        Console.WriteLine("");
        Console.WriteLine(new string('-', 20));
        Console.Write("Enter task name: ");
        var name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
            return;

        Console.WriteLine("");
        Console.WriteLine("Select Category:");

        foreach (var category in _service.Categories)
            Console.WriteLine($"{category.Id}. {category.Name}");

        Console.WriteLine("");
        Console.WriteLine(new string('-', 20));
        Console.Write("Category: ");

        var input = Console.ReadLine();

        if (int.TryParse(input, out int categoryId))
        {
            var started = _service.StartTask(name, categoryId);

            if (!started)
                Console.WriteLine("Invalid category or task already running.");
        }
        else
        {
            var newCategory = _service.AddCategory(input!);
            _service.StartTask(name, newCategory.Id);
        }
    }

    private void ShowWorkingMenu()
    {
        while (_service.CurrentTask != null)
        {
            var task = _service.CurrentTask!;

            Console.WriteLine("\n\n");
            Console.WriteLine(new string('=', 30));
            Console.WriteLine($"Task: {task.Task.Name}");
            Console.WriteLine($"State: {task.State}");
            Console.WriteLine($"Elapsed: {task.Duration:hh\\:mm\\:ss}");
            Console.WriteLine(new string('=', 30));
            Console.WriteLine();

            Console.WriteLine("1. Complete");
            Console.WriteLine(task.State == TaskState.Running ? "2. Pause" : "2. Resume");
            Console.WriteLine("3. Discard");
            Console.Write("Select option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    _service.CompleteTask();
                    Console.WriteLine("\nTask completed.");
                    Console.WriteLine($"Final Time: {task.Duration:hh\\:mm\\:ss}");
                    Pause();
                    return;

                case "2":
                    if (task.State == TaskState.Running)
                    {
                        _service.PauseTask();
                        Console.WriteLine("\nTask paused.");
                    }
                    else
                    {
                        _service.ResumeTask();
                        Console.WriteLine("\nTask resumed.");
                    }
                    break;

                case "3":
                    _service.DiscardTask();
                    Console.WriteLine("\nTask discarded.");
                    Pause();
                    return;
            }
        }
    }

    private void ShowDailySummary()
    {
        var logs = _service.GetTodayTasks();
        var summary = new DailySummary(DateTime.Today, logs);

        summary.Display();
        Pause();
    }

    private void ShowWeeklySummary()
    {
        var summary = new WeeklySummary(_service);

        summary.DisplaySelection(_service);
        Pause();
    }

    private void Pause()
    {
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }
}