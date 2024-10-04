namespace SchemaMigrator.Database.Core.Models;

public class SchemaDescriptor
{
    public required string SchemaName { get; set; } = null!;
    public List<FieldDescriptor> Fields { get; set; } = [];

    public void AddField(FieldDescriptor field)
    {
        if (Fields.All(descriptor => descriptor.Name != field.Name))
        {
            Fields.Add(field);
        }
        else
        {
            throw new ArgumentException("Schema descriptor already contains a field with such name");
        }
    }

    public void RemoveField(string fieldName)
    {
        var existedField = Fields.First(descriptor => descriptor.Name == fieldName);
        Fields.Remove(existedField);
    }

    public bool HasField(string fieldName)
    {
        return Fields.Any(descriptor => descriptor.Name == fieldName);
    }
}