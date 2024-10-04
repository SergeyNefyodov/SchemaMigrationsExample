using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Core;
using SchemaMigrator.Database.Core.Models;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database;

public class MigrationBuilder
{
    private List<SchemaDescriptor> _schemas = [];
    private List<SchemaBuilderData> _buildersData = [];
    
    public void CreateSchema(SchemaBuilderData data, SchemaDescriptor descriptor)
    {
        _buildersData.Add(data);
        _schemas.Add(descriptor);
    }

    public void UpdateGuid(string schemaName, Guid newGuid)
    {
        _buildersData.First(x => x.Name == schemaName).Guid = newGuid;
    }

    public void AddColumn(string tableName, string name, Type fieldType)
    {
        _schemas.First(schema => schema.SchemaName == tableName).AddField(new FieldDescriptor(name, fieldType));
    }
    
    public void DropColumn(string tableName, string name)
    {
        _schemas.First(schema => schema.SchemaName == tableName).RemoveField(name);
    }

    public List<Schema> Migrate(Dictionary<string, Guid> lastExistedGuids)
    {
        var result = new List<Schema>();
        foreach (var guidPair in lastExistedGuids)
        {
            var existingSchema = Schema.Lookup(guidPair.Value);
            var data = _buildersData.First(data => data.Name == guidPair.Key);
            var schemaDescriptor = _schemas.First(schema => schema.SchemaName == guidPair.Key);
        
            var builder = new SchemaBuilder(data.Guid)
                .SetSchemaName(data.Name)
                .SetDocumentation(data.Documentation)
                .SetVendorId(data.VendorId);
        
            foreach (var field in schemaDescriptor.Fields)
            {
                var propertyType = field.Type;

                if (propertyType.IsGenericType)
                {
                    var genericTypeDefinition = propertyType.GetGenericTypeDefinition();

                    if (genericTypeDefinition == typeof(List<>))
                    {
                        var elementType = propertyType.GetGenericArguments()[0]; 
                        builder.AddArrayField(field.Name, elementType);
                    }
                    else if (genericTypeDefinition == typeof(Dictionary<,>))
                    {
                        var genericArgs = propertyType.GetGenericArguments();
                        var keyType = genericArgs[0];   
                        var valueType = genericArgs[1]; 
                        builder.AddMapField(field.Name, keyType, valueType);
                    }
                }
                else
                {
                    builder.AddSimpleField(field.Name, propertyType);
                }
            }

            var resultSchema = builder.Finish();
            if (existingSchema is not null && SchemaUtils.HasElements(existingSchema, Context.ActiveDocument!))
            {
                EntityMigrator.Migrate(existingSchema, resultSchema);
            }
            result.Add(resultSchema);
        }
        return result;
    }

    public List<FieldDescriptor> GetColumns(string tableName)
    {
        return _schemas.First(schema => schema.SchemaName == tableName).Fields;
    }
}