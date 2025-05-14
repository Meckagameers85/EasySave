using System.Text.Json;

namespace EasySaveProject.Models;

public class LanguageManager
{
    private Dictionary<string, string> _translations = new();

    public void Load(string newLanguageCode)
    {
        /* 
            - Visibility : public
            - Input : string newLanguageCode
            - Output : None
            - Description : Loads the translations from a JSON file based on the provided language code.
        */
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
        /* 
            - Visibility : public
            - Input : string key
            - Output : string
            - Description : Translates the provided key using the loaded translations. If the key is not found, returns a placeholder.
        */
        return _translations.TryGetValue(key, out var value) ? value : $"[missing:{key}]";
    }
}
