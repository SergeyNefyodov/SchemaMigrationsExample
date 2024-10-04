﻿using SchemaMigrator.Database.Models;

namespace ConsoleMigrationTool
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                args = Console.ReadLine().Split(' ');
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (args.Length < 2) 
            {
                Console.WriteLine("Please specify a command and a project. Available commands: add <MigrationName>");
                return;
            }

            var command = args[0].ToLower();

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

                    MigrationTool.MigrationTool.AddMigration(migrationName, typeof(Person));
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}. Available commands: add <MigrationName>");
                    break;
            }
        }
    }
}