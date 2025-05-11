public interface ILocalizedResources
{
    string GetString(string key);
}

public static class LocalizedResourcesFactory
{
    public static ILocalizedResources Create(string lang)
    {
        return lang switch
        {
            "en" => new EnglishResources(),
            "fr" => new FrenchResources(),
            _ => throw new NotSupportedException("Langue non supportée")
        };
    }
}