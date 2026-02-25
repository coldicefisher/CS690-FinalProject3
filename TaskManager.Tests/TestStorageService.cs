using Xunit;
using TaskManager;
using System.IO;
using System.Linq;




public class StorageServiceTests {
    private string CreateTempFile() {
        return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    }

    [Fact]
    public void Load_ReturnsEmpty_WhenFileDoesNotExist() {
        var path = CreateTempFile();
        var service = new StorageService(path, path + "_cat");

        var logs = service.Load();

        Assert.Empty(logs);
    }



    [Fact]
    public void Save_Then_Load_RoundTripsData() {
        var logPath = CreateTempFile();
        var catPath = CreateTempFile();

        var service = new StorageService(logPath, catPath);

        var logs = new List<TaskLog> {
            new TaskLog {
                Id = 1,
                Task = new UserTask { Name = "Test" },
                StartTime = DateTime.Now,
                State = TaskState.Completed
            }
        };

        service.Save(logs);

        var loaded = service.Load();

        Assert.Single(loaded);
        Assert.Equal("Test", loaded.First().Task.Name);

        File.Delete(logPath);
    }

    [Fact]
    public void SaveCategories_Then_LoadCategories_RoundTripsData() {
        var logPath = CreateTempFile();
        var catPath = CreateTempFile();

        var service = new StorageService(logPath, catPath);

        var categories = new List<Category> {
            new Category { Id = 1, Name = "Work" }
        };

        service.SaveCategories(categories);

        var loaded = service.LoadCategories();

        Assert.Single(loaded);
        Assert.Equal("Work", loaded.First().Name);

        File.Delete(catPath);
    }
}