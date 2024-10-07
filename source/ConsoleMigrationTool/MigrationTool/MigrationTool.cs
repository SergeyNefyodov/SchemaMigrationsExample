using SchemaMigrator.Database;
using SchemaMigrator.Database.Core.Models;

namespace ConsoleMigrationTool.MigrationTool;

public class MigrationTool
{
    public static void AddMigration(string migrationName)
    {
        var types = FindModelTypes();
        if (types.Count == 0) return;
        var snapshots = GetLastMigrationsSnapshot(types.Keys.ToArray());
        var generator = new MigrationGenerator(types.Values.ElementAt(0));
        for (var i =0; i<types.Count; i++)
        {
            var pair = types.ElementAt(i);
            var schemaName = pair.Key;
            var type = pair.Value;
            var snapshot = snapshots.FirstOrDefault(snapshot => snapshot.SchemaName == schemaName);
            if (snapshot is null || snapshot.Fields.Count == 0)
            {
                generator.AddInitialMigration(schemaName, pair.Value);
            }
            else
            {
                var changes = ChangeDetector.DetectChanges(pair.Value, snapshot.Fields.ToDictionary(field => field.Name, field => field.Type));
                generator.AddMigration(migrationName, changes, type);
            }
        }
        generator.Finish(migrationName);
    }

    private static List<SchemaDescriptor> GetLastMigrationsSnapshot(string[] schemaNames)
    {
        var fields = new List<SchemaDescriptor>();
        var migrationTypes = GetMigrationTypes();
        if (!migrationTypes.Any()) return fields;

        // Assuming migrations are named like "20241003_1009_MigrationName" 
        var lastMigrationType = migrationTypes.OrderByDescending(t => t.Name).FirstOrDefault(); // TODO: add sorting by filename

        if (lastMigrationType == null) return fields;

        var migrationInstance = (Migration)Activator.CreateInstance(lastMigrationType);
        var migrationBuilder = new MigrationBuilder();

        migrationInstance.Up(migrationBuilder);
        foreach (var name in schemaNames)
        {
            var schemaDescriptor = new SchemaDescriptor(name)
            {
                Fields = migrationBuilder.GetColumns(name)
            };
            fields.Add(schemaDescriptor);
        }

        return fields;
    }

    private static Type[] GetMigrationTypes()
    {
        var migrationTypes =  typeof(SchemaContext).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Migration)))
            .ToArray();

        return migrationTypes;
    }
    
    private static Dictionary<string, Type> FindModelTypes()
    {
        var types = new Dictionary<string, Type>();
        var contextType = typeof(SchemaContext);
        var propertyInfos = contextType
            .GetProperties()
            .Where(property => property.PropertyType.GetGenericTypeDefinition() == typeof(SchemaSet<>));

        foreach (var propertyInfo in propertyInfos)
        {
            types.Add(propertyInfo.Name, propertyInfo.PropertyType.GetGenericArguments()[0]);
        }
        return types;
    }
}