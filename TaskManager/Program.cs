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

    static void Pause()
    {
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey();
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
        var logs = service.GetTodayTasks();

        var summary = new DailySummary(DateTime.Today, logs);

        summary.Display();

        Pause();
    }

    static void ShowWeeklySummary(TaskService service)
    {
        var weeklyGroups = service.GetWeeklyGroups();

        var summary = new WeeklySummary(weeklyGroups);

        summary.DisplaySelection(service);

        Pause();
    }


    
    
}

