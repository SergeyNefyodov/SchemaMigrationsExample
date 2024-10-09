using System.Reflection;
using ConsoleMigrationTool.Models;

namespace ConsoleMigrationTool.MigrationTool;

public class MigrationTool
{
    public static void AddMigration(string migrationName, string projectName)
    {
        Console.WriteLine(projectName);
        var solutionDirectory = PathUtils.GetSolutionDirectory(Assembly.GetEntryAssembly());
        Console.WriteLine(solutionDirectory);
        var dllPath = string.Empty;
        foreach (var dir in Directory.GetDirectories(solutionDirectory, $"*{projectName}*", searchOption: SearchOption.AllDirectories))
        {
            dllPath = Directory.GetFiles(dir, $"{projectName}.dll", SearchOption.AllDirectories)[0];
            break;
        }
        //
        //Console.WriteLine(dllPath);
        
        
        
        var assembly = Assembly.LoadFile(dllPath);
        var types = FindModelTypes(projectName, assembly);
        if (types.Count == 0) return;
        var snapshots = GetLastMigrationsSnapshot(types.Keys.ToArray(), assembly);
        var generator = new MigrationGenerator(types.Values.ElementAt(0));
        for (var i = 0; i < types.Count; i++)
        {
            var pair = types.ElementAt(i);
            var schemaName = pair.Key;
            var type = pair.Value;
            var snapshot = snapshots.FirstOrDefault(snapshot => snapshot.SchemaName == schemaName);
            if (snapshot is null || snapshot.Fields.Count == 0)
            {
                generator.AddInitialMigration(schemaName, type);
            }
            else
            {
                var changes = ChangeDetector.DetectChanges(type, schemaName, snapshot.Fields.ToDictionary(field => field.Name, field => field.Type));
                if (changes.Any())
                {
                    generator.AddMigration(schemaName, changes);
                }
            }
        }

        generator.Finish(migrationName);
    }

    private static List<SchemaDescriptor> GetLastMigrationsSnapshot(string[] schemaNames, Assembly assembly)
    {
        var schemas = new List<SchemaDescriptor>();
        var migrationTypes = GetMigrationTypes(assembly);
        if (!migrationTypes.Any()) return schemas;
        var migrationBuilderType = assembly.GetTypes().First(type => type.Name == "MigrationBuilder");
        var migrationBuilder = Activator.CreateInstance(migrationBuilderType);
        foreach (var migrationType in migrationTypes)
        {
            var migrationInstance = Activator.CreateInstance(migrationType);
            var method = migrationType.GetMethod("Up");
            method!.Invoke(migrationInstance, [migrationBuilder]);
            foreach (var name in schemaNames)
            {
                var getColumnsMethod = migrationBuilderType.GetMethod("GetColumns");
                var schemaDescriptor = new SchemaDescriptor(name)
                {
                    Fields = (List<FieldDescriptor>)getColumnsMethod!.Invoke(migrationBuilder, [name])
                };
                schemas.Add(schemaDescriptor);
            }
        }

        return schemas;
    }

    private static Type[] GetMigrationTypes(Assembly assembly)
    {
        var migrationTypes = assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("Migration"))
            .OrderBy(migrationType =>
            {
                var className = migrationType.Name;
                return className.Remove(0, className.IndexOf('_'));
            })
            .ToArray();

        return migrationTypes;
    }

    private static Dictionary<string, Type> FindModelTypes(string projectName, Assembly assembly)
    {
        var type = assembly.GetType($"{projectName}.Database.ApplicationSchemaContext");
        // var types = assembly.GetTypes().Where(type => type.Name.EndsWith("SchemaContext")).ToArray();
        // if (types.Count() != 1)
        // {
        //     throw new ArgumentException("Project must contain exactly one SchemaContext.");
        // }

        var typesDictionary = new Dictionary<string, Type>();
        var contextType = type;
        var propertyInfos = contextType
            .GetProperties()
            .Where(property => property.PropertyType.GetGenericTypeDefinition().Name.StartsWith("SchemaSet"));

        foreach (var propertyInfo in propertyInfos)
        {
            typesDictionary.Add(propertyInfo.Name, propertyInfo.PropertyType.GetGenericArguments()[0]);
        }

        return typesDictionary;
    }
}