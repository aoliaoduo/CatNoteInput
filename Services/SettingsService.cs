using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CatNoteInput;

public sealed class SettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public SettingsService()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsPath = Path.Combine(root, "CatNoteInput", "settings.json");
    }

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            return new AppSettings();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        var folder = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var json = JsonSerializer.Serialize(settings, _serializerOptions);
        await File.WriteAllTextAsync(_settingsPath, json).ConfigureAwait(false);
    }
}
