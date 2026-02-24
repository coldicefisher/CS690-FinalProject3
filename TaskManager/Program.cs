namespace TaskManager;

using Spectre.Console;


class Program
{
    static void Main(string[] args)
    {
        var taskService = new TaskService();
        bool running = true;

        // Main loop is exited whenever the User chooses to exit- setting running to false.
        while (running)
        {
            // If a task is currently active, show working screen
            if (taskService.CurrentTask != null)
            {
                ShowWorkingMenu(taskService);
                continue;
            }

            // Main menu ///////////////////////////////////////////////////////////////////////////
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
                    StartTask(taskService);
                    break;

                case "2":
                    ShowDailySummary(taskService);
                    break;

                case "3":
                    ShowWeeklySummary(taskService);
                    break;

                case "4":
                    running = false;
                    break;
            }
            // END Main Menu //////////////////////////////////////////////////////////////////////////
        } // End of main loop
    }

    static void StartTask(TaskService service)
    {
        Console.WriteLine("");
        Console.WriteLine(new string('-', 20));    
        Console.Write("Enter task name: ");
        var name = Console.ReadLine();


        if (string.IsNullOrWhiteSpace(name))
            return;

        Console.WriteLine("");
        Console.WriteLine("Select Category:");
        foreach (var category in service.Categories)
        {
            Console.WriteLine($"{category.Id}. {category.Name}");
        }
        Console.WriteLine("");
        Console.WriteLine(new string('-', 20));    
        Console.Write("Category: ");
        var input = Console.ReadLine();

        if (int.TryParse(input, out int categoryId))
        {
            bool started = service.StartTask(name, categoryId);

            if (!started)
                Console.WriteLine("Invalid category or task already running.");
        }
        else
        {
            var newCategory = service.AddCategory(input!);
            service.StartTask(name, newCategory.Id);
        }
    }

    static void ShowWorkingMenu(TaskService service)
    /**
        * This menu is shown when a task is active. It allows the user to:
        * 1. Complete the task (which saves it and returns to main menu)
        * 2. Pause/Resume the task (toggles between pause and resume based on current state)
        * 3. Discard the task (deletes it and returns to main menu)
    */
    {
        while (service.CurrentTask != null)
        {
            var task = service.CurrentTask!;
            Console.WriteLine("\n\n");
            Console.WriteLine(new string('=', 30));
            Console.WriteLine($"Task: {task.Task.Name}");
            Console.WriteLine($"State: {task.State}");
            Console.WriteLine($"Elapsed: {task.Duration:hh\\:mm\\:ss}");
            Console.WriteLine(new string('=', 30)); 
            Console.WriteLine();

            Console.WriteLine("1. Complete");
            // Show "Pause" if currently running, otherwise show "Resume". We can use a ternary here. 
            Console.WriteLine(task.State == TaskState.Running ? "2. Pause" : "2. Resume");
            Console.WriteLine("3. Discard");
            Console.Write("Select option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    service.CompleteTask();
                    Console.WriteLine("\nTask completed.");
                    Console.WriteLine($"Final Time: {task.Duration:hh\\:mm\\:ss}");
                    Pause();
                    return;

                case "2":
                    if (task.State == TaskState.Running)
                    {
                        service.PauseTask();
                        Console.WriteLine("\nTask paused.");
                    }
                    else
                    {
                        service.ResumeTask();
                        Console.WriteLine("\nTask resumed.");
                    }
                    break;

                case "3":
                    service.DiscardTask();
                    Console.WriteLine("\nTask discarded.");
                    Pause();
                    return;
            }
        }
    }

    static void ShowDailySummary(TaskService service)
    {
        var tasks = service.GetTodayTasks();

        // Write Header
        Console.WriteLine();
        Console.WriteLine(new string('*', 30));
        Console.WriteLine("Daily Summary");
        Console.WriteLine(new string('*', 30));
        Console.WriteLine();

        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks recorded today.");
            Pause();
            return;
        }

        Console.WriteLine("+------------------------------+------------------+----------------+");
        Console.WriteLine("| Task Name                    | Category         | Time Spent     |");
        Console.WriteLine("+------------------------------+------------------+----------------+");

        foreach (var task in tasks)
        {
            var formattedDuration = task.Duration.ToString(@"hh\:mm\:ss");
            var categoryName = task.Task.Category?.Name ?? "Uncategorized";

            Console.WriteLine($"| {task.Task.Name,-28} | {categoryName,-16} | {formattedDuration,-14} |");
        }

        Console.WriteLine("+------------------------------+------------------+----------------+");

        Pause();
    }


    static void ShowWeeklySummary(TaskService service)
    {
        while (true)
        {
            var weeklyGroups = service.GetWeeklyGroups();

            if (!weeklyGroups.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No weekly data available.[/]");
                Pause();
                return;
            }

            var prompt = new SelectionPrompt<string>()
                .Title("Select a week:")
                .PageSize(10);

            
            prompt.AddChoice("Cancel");
            
            foreach (var group in weeklyGroups)
            {
                var label = $"{group.Key.Start:yyyy-MM-dd} to {group.Key.End:yyyy-MM-dd}";
                prompt.AddChoice(label);
            }

            

            var selected = AnsiConsole.Prompt(prompt);

            if (selected == "Cancel")
                return;

            var selectedIndex = weeklyGroups.FindIndex(g =>
                $"{g.Key.Start:yyyy-MM-dd} to {g.Key.End:yyyy-MM-dd}" == selected);

            var selectedWindow = weeklyGroups
                .Skip(selectedIndex)
                .Take(4)
                .ToList();

            var oldestWeek = selectedWindow.Last().Key.Start;
            var newestWeek = selectedWindow.First().Key.End;

            AnsiConsole.Clear();

            AnsiConsole.Write(new Rule(
                $"[bold yellow]TOTAL AGGREGATES ({oldestWeek:yyyy-MM-dd} to {newestWeek:yyyy-MM-dd})[/]"));

            DisplayAggregateSection(selectedWindow.SelectMany(g => g));

            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Rule("[bold cyan]WEEKLY BREAKDOWN[/]"));

            foreach (var week in selectedWindow)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(
                    $"[bold]Week {week.Key.Start:yyyy-MM-dd} to {week.Key.End:yyyy-MM-dd}[/]");

                DisplayAggregateSection(week);
            }

            // Bottom Menu
            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\nSelect option:")
                    .AddChoices("Delete Entry", "Return"));

            if (action == "Return")
                return;

            HandleDeleteFlow(service);
        }
    }

    static void DisplayAggregateSection(IEnumerable<TaskLog> logs)
    {
        var categoryTable = new Table();
        categoryTable.AddColumn("Category");
        categoryTable.AddColumn("Time Spent");

        var categoryGroups = logs
            .GroupBy(t => t.Task.Category?.Name ?? "Uncategorized")
            .Select(g => new
            {
                Category = g.Key,
                Total = TimeSpan.FromTicks(g.Sum(t => t.Duration.Ticks))
            })
            .OrderByDescending(x => x.Total);

        foreach (var cat in categoryGroups)
        {
            categoryTable.AddRow(cat.Category, cat.Total.ToString(@"hh\:mm\:ss"));
        }

        AnsiConsole.Write(categoryTable);
        AnsiConsole.WriteLine();

        var taskTable = new Table();
        taskTable.AddColumn("Task");
        taskTable.AddColumn("Time Spent");

        var taskGroups = logs
            .GroupBy(t => t.Task.Name)
            .Select(g => new
            {
                Task = g.Key,
                Total = TimeSpan.FromTicks(g.Sum(t => t.Duration.Ticks))
            })
            .OrderByDescending(x => x.Total);

        foreach (var task in taskGroups)
        {
            taskTable.AddRow(task.Task, task.Total.ToString(@"hh\:mm\:ss"));
        }

        AnsiConsole.Write(taskTable);
    }

    static void HandleDeleteFlow(TaskService service)
    {
        var allLogs = service.GetWeeklyGroups()
                            .SelectMany(g => g)
                            .OrderByDescending(l => l.StartTime)
                            .ToList();

        if (!allLogs.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No entries to delete.[/]");
            Pause();
            return;
        }

        // SELECT DAY //////////////////////////////////////////////////////////
        var days = allLogs
            .Select(l => l.StartTime.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var dayPrompt = new SelectionPrompt<string>()
            .Title("Select a day:")
            .PageSize(10)
            .UseConverter(x => x); // disables markup parsing

        dayPrompt.AddChoice("Cancel");

        foreach (var day in days)
            dayPrompt.AddChoice(day.ToString("yyyy-MM-dd"));


        var selectedDay = AnsiConsole.Prompt(dayPrompt);

        if (selectedDay == "Cancel")
            return;

        var parsedDay = DateTime.Parse(selectedDay);

        var logsForDay = allLogs
            .Where(l => l.StartTime.Date == parsedDay)
            .OrderByDescending(l => l.StartTime)
            .ToList();

        if (!logsForDay.Any())
            return;

        
        // SELECT ENTRY TO DELETE //////////////////////////////////////////////////////////
        var entryPrompt = new SelectionPrompt<string>()
            .Title("Select entry to delete:")
            .PageSize(15)
            .UseConverter(x => x); // disables markup parsing

        entryPrompt.AddChoice("Cancel");

        foreach (var log in logsForDay)
        {
            var label =
                $"{log.Id} | {log.StartTime:HH:mm} | {log.Task.Name} | {log.Duration:hh\\:mm\\:ss}";

            entryPrompt.AddChoice(label);
        }

        var selectedEntry = AnsiConsole.Prompt(entryPrompt);

        if (selectedEntry == "Cancel")
            return;

        var id = int.Parse(selectedEntry.Split('|')[0].Trim());

        service.DeleteTaskLog(id);

        AnsiConsole.MarkupLine("[red]Entry deleted successfully.[/]");
        Pause();
    }

    static void Pause()
    {
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
    }
}

