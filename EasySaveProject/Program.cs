using EasySaveProject.ViewModels;
using EasySaveProject.Views;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 1 && (args[0] == "-h" || args[0] == "-help"))
        {
            ShowHelp();
            return;
        }

        else if (args.Length == 3)
        {
            string source = args[0];
            string target = args[1];
            string type = args[2];
            if (!Directory.Exists(source))
            {
                Console.WriteLine($"Source directory not found: {source}");
                return;
            }

            // Vérifier que le chemin cible n'est pas vide et contient uniquement des caractères valides
            if (string.IsNullOrWhiteSpace(target) || target.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                Console.WriteLine("Le chemin de destination est vide ou contient des caractères invalides.");
                return;
            }

            if (type != "Full" && type != "Differential")
            {
                Console.WriteLine($"Type must be 'Full' or 'Differential'.");
                return;
            }

            var task = new SaveTask
            {
                name = "CLI_Backup",
                sourceDirectory = source,
                targetDirectory = target,
                type = type == "Differential" ? SaveType.Differential : SaveType.Full
            };

            task.Run();
            Console.WriteLine("Backup completed.");
        }
        else
        {
            // Mode interactif
            var viewModel = new MenuViewModel();
            var view = new MainMenuView(viewModel);
            view.Show();
        }
    }
    static void ShowHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  EasySaveProject.exe <Source> <Target> <Type>");
        Console.WriteLine("  <Source> : Directory to back up");
        Console.WriteLine("  <Target> : Destination directory");
        Console.WriteLine("  <Type>   : 'Full' or 'Differential'");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  EasySaveProject.exe C:\\MyData C:\\MyBackup Full");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -h, -help       Show this help message");
    }
}
