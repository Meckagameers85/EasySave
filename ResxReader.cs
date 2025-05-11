using System.Globalization;
using System.Resources;
using System.Reflection;

public class ResxReader
{
    private readonly ResourceManager _manager;
    private readonly CultureInfo _culture;

    public ResxReader(string baseName, string cultureCode = "en")
    {
        _manager = new ResourceManager(baseName, Assembly.GetExecutingAssembly());
        _culture = new CultureInfo(cultureCode);
    }

    public string GetString(string key, params object[] args)
    {
        string? value = _manager.GetString(key, _culture);
        if (value == null)
            return $"[{key}]"; // fallback si clé inconnue

        return args.Length > 0 ? string.Format(value!, args) : value!;
    }
    public IEnumerable<string> GetAllKeys()
    {
        var resourceSet = _manager.GetResourceSet(_culture, true, true);
        foreach (System.Collections.DictionaryEntry entry in resourceSet!)
        {
            yield return (string)entry.Key;
        }
    }
}
