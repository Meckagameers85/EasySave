public class EnglishResources : ILocalizedResources
{
    private readonly ResxReader _reader;

    public EnglishResources()
    {
        _reader = new ResxReader("CLIVersion.Properties.strings", "en");
    }

    string ILocalizedResources.GetString(string key)
    {
        var reader = new ResxReader("CLIVersion.properties.strings", "en");
        string message = reader.GetString("AppWelcome");
        return message;
        throw new NotImplementedException();
    }
    public IEnumerable<string> GetAllKeys()
    {
        return _reader.GetAllKeys();
    }
    
}