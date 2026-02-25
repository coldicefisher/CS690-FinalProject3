using Xunit;
using TaskManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskManager.Tests;

public class DailySummaryTests
{

    private TaskLog CreateLog(string name, DateTime start, TimeSpan duration)
    {
        return new TaskLog
        {
            Id = 1,
            Task = new UserTask { Name = name },
            StartTime = start,
            TotalActiveTime = duration,
            State = TaskState.Completed
        };
    }




    [Fact]
    public void Constructor_NormalizesDate() {
        var inputDate = new DateTime(2024, 3, 5, 14, 30, 0);

        var summary = new DailySummary(inputDate, new List<TaskLog>());

        Assert.Equal(new DateTime(2024, 3, 5), summary.Date);
    }

    [Fact]
    public void HasData_ReturnsFalse_WhenNoLogsMatchDate() {
        var logs = new List<TaskLog>
        {
            CreateLog("TaskA", DateTime.Today.AddDays(-1), TimeSpan.FromMinutes(30))
        };

        var summary = new DailySummary(DateTime.Today, logs);

        Assert.False(summary.HasData());
    }


    [Fact]
    public void HasData_ReturnsTrue_WhenLogsMatchDate() {
        var logs = new List<TaskLog>
        {
            CreateLog("TaskA", DateTime.Today, TimeSpan.FromMinutes(30))
        };

        var summary = new DailySummary(DateTime.Today, logs);

        Assert.True(summary.HasData());
    }

    [Fact]
    public void FiltersLogsByDate() {
        var logs = new List<TaskLog>
        {
            CreateLog("TodayTask", DateTime.Today, TimeSpan.FromMinutes(30)),
            CreateLog("YesterdayTask", DateTime.Today.AddDays(-1), TimeSpan.FromMinutes(60))
        };

        var summary = new DailySummary(DateTime.Today, logs);

        var totals = summary.GetTaskTotals().ToList();

        Assert.Single(totals);
        Assert.Equal("TodayTask", totals[0].TaskName);
    }



    [Fact]
    public void AggregatesMultipleEntriesForSameTask() {
        var logs = new List<TaskLog>
        {
            CreateLog("TaskA", DateTime.Today, TimeSpan.FromMinutes(30)),
            CreateLog("TaskA", DateTime.Today, TimeSpan.FromMinutes(45))
        };

        var summary = new DailySummary(DateTime.Today, logs);

        var totals = summary.GetTaskTotals().ToList();

        Assert.Single(totals);
        Assert.Equal(TimeSpan.FromMinutes(75), totals[0].Total);
    }



    [Fact]
    public void ReturnsTasksOrderedByDescendingTime() {
        var logs = new List<TaskLog>
        {
            CreateLog("TaskA", DateTime.Today, TimeSpan.FromMinutes(30)),
            CreateLog("TaskB", DateTime.Today, TimeSpan.FromMinutes(60))
        };

        var summary = new DailySummary(DateTime.Today, logs);

        var totals = summary.GetTaskTotals().ToList();

        Assert.Equal("TaskB", totals[0].TaskName);
        Assert.Equal("TaskA", totals[1].TaskName);
    }

    
}