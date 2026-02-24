using System.Text.Json;
using TaskManager;

namespace TaskManager;

public class StorageService
{
    private readonly string _filePath = "tasklogs.json";
    private readonly string _categoryPath = "categories.json";



    public List<TaskLog> Load()
    {
        if (!File.Exists(_filePath))
            return new List<TaskLog>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<TaskLog>>(json) ?? new();
    }

    public void Save(List<TaskLog> logs)
    {
        var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
    }

    public List<Category> LoadCategories()
    {
        if (!File.Exists(_categoryPath))
            return new List<Category>();

        var json = File.ReadAllText(_categoryPath);
        return JsonSerializer.Deserialize<List<Category>>(json) ?? new();
    }

    public void SaveCategories(List<Category> categories)
    {
        var json = JsonSerializer.Serialize(categories, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_categoryPath, json);
    }

}