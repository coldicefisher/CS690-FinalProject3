namespace TaskManager;

using Spectre.Console;

public class DailySummary
{
    private readonly IEnumerable<TaskLog> _logs;

    public DateTime Date { get; }

    public DailySummary(DateTime date, IEnumerable<TaskLog> logs) {
        Date = date.Date;
        _logs = logs
            .Where(l => l.StartTime.Date == Date)
            .ToList();
    }

    public bool HasData() {
        return _logs.Any();
    }

    

    public IEnumerable<(string TaskName, TimeSpan Total)> GetTaskTotals() {
        return _logs
            .GroupBy(l => l.Task.Name)
            .Select(g => (
                TaskName: g.Key,
                Total: TimeSpan.FromTicks(g.Sum(l => l.Duration.Ticks))
            ))
            .OrderByDescending(x => x.Total);
    }



    public void Display() {
        AnsiConsole.Clear();

        AnsiConsole.Write(new Rule($"[bold yellow]Daily Summary - {Date:yyyy-MM-dd}[/]"));

        if (!HasData())
        {
            AnsiConsole.MarkupLine("[yellow]No tasks recorded today.[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("Task Name");
        table.AddColumn("Time Spent");

        var totals = GetTaskTotals(); 

        foreach (var item in totals)
        {
            table.AddRow(
                item.TaskName,
                item.Total.ToString(@"hh\:mm\:ss")
            );
        }

        AnsiConsole.Write(table);
    }

}