using SchemaMigrator.Database;

namespace ConsoleMigrationTool.MigrationTool;

public class MigrationTool
{
    public static void AddMigration(string migrationName, Type type)
    {
        var a = FindModelTypes();
        var lastMigrationSnapshot = GetLastMigrationSnapshot(type);

        if (!lastMigrationSnapshot.Any())
        {
            MigrationGenerator.GenerateInitialMigration(migrationName, type);
        }
        else
        {
            var changes = ChangeDetector.DetectChanges(type, lastMigrationSnapshot);
            MigrationGenerator.GenerateMigration(migrationName, changes, type);
        }
    }

    private static Dictionary<string, Type> GetLastMigrationSnapshot(Type type)
    {
        var dictionary = new Dictionary<string, Type>();
        var migrationTypes = GetMigrationTypes(type);
        if (!migrationTypes.Any()) return dictionary;

        // Assuming migrations are named like "20241003_1009_MigrationName"
        var lastMigrationType = migrationTypes.OrderByDescending(t => t.Name).FirstOrDefault();

        if (lastMigrationType == null) return dictionary;

        var migrationInstance = (Migration)Activator.CreateInstance(lastMigrationType);
        var migrationBuilder = new MigrationBuilder();

        migrationInstance.Up(migrationBuilder);
        dictionary = migrationBuilder.GetColumns();

        return dictionary;
    }

    private static Type[] GetMigrationTypes(Type type)
    {
        var migrationTypes =  type.Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Migration)))
            .ToArray();

        return migrationTypes;
    }
    
    private static List<Type> FindModelTypes()
    {
        var types = new List<Type>();
        var contextType = typeof(SchemaContext);
        var propertyInfos = contextType
            .GetProperties()
            .Where(property => property.PropertyType.GetGenericTypeDefinition() == typeof(SchemaSet<>));

        foreach (var propertyInfo in propertyInfos)
        {
            types.Add(propertyInfo.PropertyType.GetGenericArguments()[0]);
        }
        return types;
    }
}