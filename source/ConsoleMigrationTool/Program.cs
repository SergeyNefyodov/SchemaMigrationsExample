using System.Reflection;

namespace ConsoleMigrationTool
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            try
            {
                //args = Console.ReadLine().Split(' ');
                var currentDirectory = Directory.GetCurrentDirectory();
                var currentDirectory1 = Environment.CurrentDirectory;

                if (args.Length < 3)
                {
                    Console.WriteLine("Please specify a command and a project. Available commands: add <MigrationName>");
                    return;
                }

                var command = args[0].ToLower();
                var projectName = args[2];

                switch (command)
                {
                    case "add":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Please specify the migration name. Example: add InitialMigration");
                            return;
                        }

                        var migrationName = args[1];
                        Console.WriteLine($"Adding migration: {migrationName}");

                        MigrationTool.MigrationTool.AddMigration(migrationName, projectName);
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}. Available commands: add <MigrationName>");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        
        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            var assemblyPath = Path.Combine(Directory.GetParent(args.RequestingAssembly.Location)!.FullName, assemblyName.Name + ".dll");
            Console.WriteLine($"Loading assembly: {assemblyPath}");
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }

            return null;
        }
    }
}