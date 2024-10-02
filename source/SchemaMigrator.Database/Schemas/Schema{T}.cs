using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Core;

namespace SchemaMigrator.Database.Schemas;

public class Schema<T> where T : class
{
    public string Name { get; set; }
    public string Documentation { get; set; }
    public string VendorId { get; set; }
    public string Guid { get; set; }

    public Schema Create()
    {
        var guid = new Guid(Guid);
        var schema = Schema.Lookup(guid);
        if (schema is not null)
        {
            if (!SchemaUtils.HasElements(schema, Context.ActiveDocument))
            {
                Context.ActiveDocument!.EraseSchemaAndAllEntities(schema);
            }
            else
            {
                return schema;
            }
        }

        var builder = new SchemaBuilder(guid)
            .SetSchemaName(Name)
            .SetDocumentation(Documentation)
            .SetVendorId(VendorId)
            .SetReadAccessLevel(AccessLevel.Public)
            .SetWriteAccessLevel(AccessLevel.Public);

        return BuildSchema(builder, typeof(T));
    }

    public void Delete()
    {
        var schema = Schema.Lookup(new Guid(Guid));
        if (schema is not null)
        {
            Context.ActiveDocument!.EraseSchemaAndAllEntities(schema);
        }
    }

    private static Schema BuildSchema(SchemaBuilder builder, Type type)
    {
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var propertyType = property.PropertyType;

            if (propertyType.IsGenericType)
            {
                var genericTypeDefinition = propertyType.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(List<>))
                {
                    var elementType = propertyType.GetGenericArguments()[0]; 
                    builder.AddArrayField(property.Name, elementType);
                }
                else if (genericTypeDefinition == typeof(Dictionary<,>))
                {
                    var genericArgs = propertyType.GetGenericArguments();
                    var keyType = genericArgs[0];   
                    var valueType = genericArgs[1]; 
                    builder.AddMapField(property.Name, keyType, valueType);
                }
            }
            else
            {
                builder.AddSimpleField(property.Name, propertyType);
            }
        }

        return builder.Finish();
    }
}