using Xunit;
using TaskManager;
using System.Linq;
using System.Threading;

namespace TaskManager.Tests;



public class TaskServiceTests
{
    private TaskService CreateService()
    {
        var fakeStorage = new FakeStorageService();
        return new TaskService(fakeStorage);
    }

    [Fact]
    public void StartTask_SetsCurrentTask()
    {
        var service = CreateService();

        var result = service.StartTask("Test Task", 1);

        Assert.True(result);
        Assert.NotNull(service.CurrentTask);
        Assert.Equal(TaskState.Running, service.CurrentTask!.State);
    }

    [Fact]
    public void StartTask_Fails_IfAlreadyRunning()
    {
        var service = CreateService();

        service.StartTask("Task1", 1);
        var result = service.StartTask("Task2", 1);

        Assert.False(result);
    }

    [Fact]
    public void PauseTask_ChangesStateToPaused()
    {
        var service = CreateService();
        service.StartTask("Task", 1);

        Thread.Sleep(10);

        service.PauseTask();

        Assert.Equal(TaskState.Paused, service.CurrentTask!.State);
        Assert.Null(service.CurrentTask.LastResumedAt);
        Assert.True(service.CurrentTask.TotalActiveTime.TotalMilliseconds > 0);
    }



    [Fact]
    public void ResumeTask_ChangesStateBackToRunning()
    {
        var service = CreateService();
        service.StartTask("Task", 1);

        service.PauseTask();
        service.ResumeTask();

        Assert.Equal(TaskState.Running, service.CurrentTask!.State);
        Assert.NotNull(service.CurrentTask.LastResumedAt);
    }



    [Fact]
    public void CompleteTask_AddsToLogs_AndClearsCurrentTask()
    {
        var service = CreateService();
        service.StartTask("Task", 1);

        Thread.Sleep(10);

        service.CompleteTask();

        Assert.Null(service.CurrentTask);

        var todayTasks = service.GetTodayTasks();
        Assert.Single(todayTasks);
        Assert.Equal(TaskState.Completed, todayTasks.First().State);
    }


    [Fact]
    public void DiscardTask_ClearsCurrentTask()
    {
        var service = CreateService();
        service.StartTask("Task", 1);

        service.DiscardTask();

        Assert.Null(service.CurrentTask);
    }


    [Fact]
    public void DeleteTaskLog_RemovesLog()
    {
        var service = CreateService();
        service.StartTask("Task", 1);
        service.CompleteTask();

        var log = service.GetTodayTasks().First();

        service.DeleteTaskLog(log.Id);

        var logs = service.GetTodayTasks();
        Assert.Empty(logs);
    }

    [Fact]
    public void AddCategory_AddsNewCategory()
    {
        var service = CreateService();

        var category = service.AddCategory("NewCat");

        Assert.Contains(service.Categories, c => c.Name == "NewCat");
    }

    [Fact]
    public void GetWeeklyGroups_ReturnsGrouping()
    {
        var service = CreateService();
        service.StartTask("Task", 1);
        service.CompleteTask();

        var groups = service.GetWeeklyGroups();

        Assert.Single(groups);
        Assert.Single(groups.First());
    }
}