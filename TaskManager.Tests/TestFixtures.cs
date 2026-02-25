using Xunit;
using TaskManager;
using System.Linq;
using System.Threading;

namespace TaskManager.Tests;


// Test Fixture to mimic storage without touching the file system
public class FakeStorageService : StorageService
{
    private List<TaskLog> _logs = new();
    private List<Category> _categories = new();

    public override List<TaskLog> Load() => _logs;
    public override void Save(List<TaskLog> logs) => _logs = logs;

    public override List<Category> LoadCategories() => _categories;
    public override void SaveCategories(List<Category> categories)
        => _categories = categories;
}
