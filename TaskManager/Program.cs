namespace TaskManager;

class Program
{
    static void Main(string[] args)
    {
        var taskService = new TaskService();
        var ui = new TaskManagerUI(taskService);

        ui.Run();
    }
}