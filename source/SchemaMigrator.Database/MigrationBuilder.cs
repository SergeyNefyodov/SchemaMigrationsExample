using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database;

public class MigrationBuilder
{
    private Dictionary<string, Type> _columns = [];
    private SchemaBuilderData _builderData;
    
    public void CreateSchema(SchemaBuilderData data, Dictionary<string, Type> fields)
    {
        _builderData = data;
        _columns = fields;
    }

    public void AddColumn(string name, Type fieldType)
    {
        _columns.Add(name, fieldType);
    }

    public Schema Migrate()
    {
        //TODO: proceed with existing schema
        
        var builder = new SchemaBuilder(_builderData.Guid)
            .SetSchemaName(_builderData.Name)
            .SetDocumentation(_builderData.Documentation)
            .SetVendorId(_builderData.VendorId);
        
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

        return builder.Finish();
    }
}