    public class FrenchResources : ILocalizedResources
{
    private readonly ResxReader _reader;

    public FrenchResources()
    {
        _reader = new ResxReader("CLIVersion.Properties.strings", "fr");
    }

    string ILocalizedResources.GetString(string key) => _reader.GetString(key) ?? $"[{key}]";
    public IEnumerable<string> GetAllKeys() => _reader.GetAllKeys();
}