class Program
{
    private static ILocalizedResources? _resources;

    static void Main()
    {
        
        Console.Write("Choisissez une langue | Choose a langage (en/fr) : ");
        var lang = Console.ReadLine();

        _resources = LocalizedResourcesFactory.Create(lang ?? "en");
    }
}