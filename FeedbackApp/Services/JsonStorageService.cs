using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeedbackApp.Services;

public class JsonStorageService<T> where T : class
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageService(string fileName)
    {
        var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        Directory.CreateDirectory(dataFolder);
        _filePath = Path.Combine(dataFolder, fileName);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }

    public async Task<List<T>> ReadAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }
        catch (JsonException)
        {
            return new List<T>();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAllAsync(List<T> items)
    {
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(items, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<T> AddAsync(T item)
    {
        await _lock.WaitAsync();
        try
        {
            var items = await ReadAllInternalAsync();
            items.Add(item);
            await SaveAllInternalAsync(items);
            return item;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RemoveAsync(Func<T, bool> predicate)
    {
        await _lock.WaitAsync();
        try
        {
            var items = await ReadAllInternalAsync();
            var itemToRemove = items.FirstOrDefault(predicate);
            if (itemToRemove == null) return false;

            items.Remove(itemToRemove);
            await SaveAllInternalAsync(items);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<T>> ReadAllInternalAsync()
    {
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
    }

    private async Task SaveAllInternalAsync(List<T> items)
    {
        var json = JsonSerializer.Serialize(items, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
