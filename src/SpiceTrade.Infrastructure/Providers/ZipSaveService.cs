using System.IO.Compression;
using System.Text.Json;
using SpiceTrade.Core.Interfaces;

namespace SpiceTrade.Infrastructure.Providers;

public sealed class ZipSaveService : ISaveService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public void Save(string path, object state)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var json = JsonSerializer.Serialize(state, JsonOptions);
            var jsonPath = Path.Combine(tempDir, "save.json");
            File.WriteAllText(jsonPath, json);

            if (File.Exists(path))
                File.Delete(path);

            ZipFile.CreateFromDirectory(tempDir, path);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    public T? Load<T>(string path)
    {
        if (!File.Exists(path))
            return default;

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            ZipFile.ExtractToDirectory(path, tempDir);
            var jsonPath = Path.Combine(tempDir, "save.json");

            if (!File.Exists(jsonPath))
                return default;

            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<T>(json);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}