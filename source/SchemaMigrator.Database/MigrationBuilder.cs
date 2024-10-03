using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Core;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database;

public class MigrationBuilder
{
    private Dictionary<string, Type> _columns = [];
    private List<SchemaBuilderData> _buildersData = [];
    
    public void CreateSchema(SchemaBuilderData data, Dictionary<string, Type> fields)
    {
        _buildersData.Add(data);
        _columns = fields;
    }

    public void UpdateGuid(string schemaName, Guid newGuid)
    {
        _buildersData.First(x => x.Name == schemaName).Guid = newGuid;
    }

    public void AddColumn(string name, Type fieldType)
    {
        _columns.Add(name, fieldType);
    }
    
    public void DropColumn(string name)
    {
        _columns.Remove(name);
    }

    public Schema Migrate(Guid lastExistedGuid)
    {
        var existingSchema = Schema.Lookup(lastExistedGuid);
        
        var builder = new SchemaBuilder(_buildersData[0].Guid)
            .SetSchemaName(_buildersData[0].Name)
            .SetDocumentation(_buildersData[0].Documentation)
            .SetVendorId(_buildersData[0].VendorId);
        
        foreach (var pair in _columns)
        {
            var propertyType = pair.Value;

            if (propertyType.IsGenericType)
            {
                var genericTypeDefinition = propertyType.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(List<>))
                {
                    var elementType = propertyType.GetGenericArguments()[0]; 
                    builder.AddArrayField(pair.Key, elementType);
                }
                else if (genericTypeDefinition == typeof(Dictionary<,>))
                {
                    var genericArgs = propertyType.GetGenericArguments();
                    var keyType = genericArgs[0];   
                    var valueType = genericArgs[1]; 
                    builder.AddMapField(pair.Key, keyType, valueType);
                }
            }
            else
            {
                builder.AddSimpleField(pair.Key, propertyType);
            }
        }

        var resultSchema = builder.Finish();
        if (existingSchema is not null && SchemaUtils.HasElements(existingSchema, Context.ActiveDocument!))
        {
            EntityMigrator.Migrate(existingSchema, resultSchema);
        }
        return resultSchema;
    }

    public Dictionary<string, Type> GetColumns()
    {
        return _columns;
    }
}