using System.Reflection;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace SchemaMigrator.Database.Schemas;

public class Schema<T> where T : class
{
    public Schema Create()
    {
        var contextType = typeof(SchemaContext);
        var propertyInfos = contextType
            .GetProperties()
            .Where(property => property.PropertyType.GetGenericTypeDefinition() == typeof(SchemaSet<>));
        var type = propertyInfos.First(info => info.PropertyType.GetGenericArguments()[0] == typeof(T));
        
        var schemaName = type.Name;
        
        var currentAssembly = Assembly.GetExecutingAssembly();

        var migrationTypes = currentAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Migration)))
            .ToArray();

        var migrationBuilder = new MigrationBuilder();
        var lastGuidDict = new Dictionary<string, Guid>();
        var lastExistedGuidDict = new Dictionary<string, Guid>();

        foreach (var migrationType in migrationTypes)
        {
            var migrationInstance = (Migration)Activator.CreateInstance(migrationType);

            if (migrationInstance is null) continue;
            
            migrationInstance.Up(migrationBuilder);
            foreach (var pair in migrationInstance.GuidDictionary)
            {
                if (!migrationInstance.GuidDictionary.TryGetValue(pair.Key, out _))
                {
                    lastGuidDict.Add(pair.Key, pair.Value);
                }
                else
                {
                    lastGuidDict[pair.Key] = pair.Value;
                }

                if (Schema.Lookup(pair.Value) is not null)
                {
                    lastExistedGuidDict[pair.Key] = pair.Value;
                }
            }
        }

        var schema = Schema.Lookup(lastGuidDict[schemaName]);
        if (schema is not null) return schema;

        var schemas = migrationBuilder.Migrate(lastExistedGuidDict);
        return schemas.Find(migratedSchema => migratedSchema.SchemaName == schemaName);
    }

    public void Delete()
    {
        // var schema = Schema.Lookup(new Guid(Guid));
        // if (schema is not null)
        // {
        //     Context.ActiveDocument!.EraseSchemaAndAllEntities(schema);
        // }
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