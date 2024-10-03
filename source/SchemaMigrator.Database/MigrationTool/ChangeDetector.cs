namespace SchemaMigrator.Database.MigrationTool;

public static class ChangeDetector
{
    public static List<string> DetectChanges(Type newModelType, Dictionary<string, Type> lastSnapshot)
    {
        var changes = new List<string>();
        var currentProperties = newModelType.GetProperties()
            .ToDictionary(prop => prop.Name, prop => prop.PropertyType);

        foreach (var pair in currentProperties)
        {
            if (!lastSnapshot.TryGetValue(pair.Key, out var value))
            {
                changes.Add($"AddColumn({pair.Key}, {pair.Value.Name})");
            }
            else if (value != pair.Value)
            {
                changes.Add($"ModifyColumn({pair.Key}, {pair.Value.Name})");
            }
        }

        foreach (var property in lastSnapshot.Keys)
        {
            if (!currentProperties.ContainsKey(property))
            {
                changes.Add($"DropColumn({property})");
            }
        }

        return changes;
    }
}