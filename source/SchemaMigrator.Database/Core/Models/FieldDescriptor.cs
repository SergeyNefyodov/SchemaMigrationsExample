namespace SchemaMigrator.Database.Core.Models;

public class FieldDescriptor(string name, Type type)
{
    public string Name { get;  } = name;
    public Type Type { get;  } = type;
}