using EasySaveProject.ViewModels;
using EasySaveProject.Views;

class Program
{
    static void Main(string[] args)
    {

        if (args.Length == 3)
        {
            string source = args[0];
            string target = args[1];
            string type = args[2];
            if (!Directory.Exists(source))
            {
                Console.WriteLine($"Source directory not found: {source}");
                return;
            }

            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
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
}
