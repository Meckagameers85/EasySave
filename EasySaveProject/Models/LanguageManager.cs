using System.Text.Json;

namespace EasySaveConsole.Models;

public class LanguageManager
{
    private Dictionary<string, string> _translations = new();

    public void Load(string newLanguageCode)
    {

        var file = $"Languages/{newLanguageCode}.json";
        if (File.Exists(file))
        {
            var json = File.ReadAllText(file);
            _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                           ?? new Dictionary<string, string>();
        }
    }

    public string Translate(string key)
    {
        return _translations.TryGetValue(key, out var value) ? value : $"[missing:{key}]";
    }
}
