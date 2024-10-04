namespace SchemaMigrator.Database.Core.Models;

public class FieldDescriptor(string name, Type type)
{
    public string Name { get; init; } = name;
    public Type Type { get; init; } = type;
}