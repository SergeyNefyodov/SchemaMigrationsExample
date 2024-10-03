using System.Reflection;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace SchemaMigrator.Database.Schemas;

public class Schema<T> where T : class
{
    public string Guid { get; set; } = "CBF9A9D4-1CD4-46DF-B1DB-EF4C49713C1D";

    public Schema Create()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        var migrationTypes = currentAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Migration)))
            .ToArray();
        
        var migrationBuilder = new MigrationBuilder();
        var lastGuid = new Guid();
        var lastExistedGuid = new Guid();

        foreach (var migrationType in migrationTypes)
        {
            var migrationInstance = (Migration)Activator.CreateInstance(migrationType);

            if (migrationInstance is not null)
            {
                migrationInstance.Up(migrationBuilder);
                lastGuid = migrationInstance.VersionGuid;
            }

            if (Schema.Lookup(lastGuid) is not null)
            {
                lastExistedGuid = lastGuid;
            }
        }
        var schema = Schema.Lookup(lastGuid);
        if (schema is not null) return schema;

        return migrationBuilder.Migrate(lastExistedGuid);
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