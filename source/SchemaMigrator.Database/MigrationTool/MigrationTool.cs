using System.Reflection;

namespace SchemaMigrator.Database.MigrationTool;

public class MigrationTool
{
    private static Dictionary<string, Type> GetLastMigrationSnapshot()
    {
        var dictionary = new Dictionary<string, Type>();
        var migrationTypes = GetMigrationTypes();
        if (!migrationTypes.Any()) return dictionary;

        // Assuming migrations are named like "20241003_1009_MigrationName"
        var lastMigrationType = migrationTypes.OrderByDescending(t => t.Name).FirstOrDefault();

        if (lastMigrationType == null) return dictionary;
        
        var migrationInstance = (Migration)Activator.CreateInstance(lastMigrationType);
        var migrationBuilder = new MigrationBuilder();

        migrationInstance.Up(migrationBuilder);
        dictionary =  migrationBuilder.GetColumns();

        return dictionary;
    }

    private static Type[] GetMigrationTypes()
    {
        var migrationTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Migration)))
            .ToArray();

        return migrationTypes;
    }
    
    public static void AddMigration(string migrationName, Type type)
    {
        var lastMigrationSnapshot = GetLastMigrationSnapshot();

        if (!lastMigrationSnapshot.Any())
        {
            MigrationGenerator.GenerateInitialMigration(migrationName, type);
        }
        else
        {
            var changes = ChangeDetector.DetectChanges(type, lastMigrationSnapshot);
            MigrationGenerator.GenerateMigration(migrationName, changes);
        }
    }
}