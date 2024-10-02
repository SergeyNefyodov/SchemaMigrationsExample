using System.Reflection;
using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Core;
using SchemaMigrator.Database.Models;

namespace SchemaMigrator.Database.Schemas;

public class DefaultSchema
{
    private const string Guid = "3F166AC0-04BD-4DAE-B71D-5068E66B1A65" ;
    private const string Name = "DefaultSchema";
    public static Schema Create()
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
            .SetDocumentation("Default schema to be updated")
            .SetVendorId("Atomatiq")
            .SetReadAccessLevel(AccessLevel.Public)
            .SetWriteAccessLevel(AccessLevel.Public);

        return BuildSchema(builder, typeof(Person));
    }

    public static void Delete()
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