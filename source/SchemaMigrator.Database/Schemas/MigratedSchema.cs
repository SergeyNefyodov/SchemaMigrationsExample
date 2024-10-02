using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Core;

namespace SchemaMigrator.Database.Schemas;

public class MigratedSchema
{
    private const string Guid = "7FBF03FC-8F85-4008-BC97-DD3461CE517A";
    private const string Name = "DefaultSchema";

    public static Schema Create()
    {
        if (TryFindSchema(out var foundSchema))
        {
            if (foundSchema.GUID.ToString() == Guid)
            {
                return foundSchema;
            }

            var newSchema = CreateNewSchema();
            EntityMigrator.Migrate(foundSchema, newSchema);
            return newSchema;
        }

        return CreateNewSchema();
    }

    private static Schema CreateNewSchema()
    {
        var guid = new Guid(Guid);

        var builder = new SchemaBuilder(guid)
            .SetSchemaName(Name)
            .SetDocumentation("Default schema to update")
            .SetVendorId("Atomatiq")
            .SetReadAccessLevel(AccessLevel.Public)
            .SetWriteAccessLevel(AccessLevel.Public);

        builder.AddSimpleField("Id", typeof(int));
        builder.AddSimpleField("Name", typeof(string));
        builder.AddSimpleField("Surname", typeof(string));
        builder.AddSimpleField("Vehicle", typeof(string));

        return builder.Finish();
    }

    private static bool TryFindSchema(out Schema foundSchema)
    {
        var schemas = Schema.ListSchemas();
        foreach (var schema in schemas)
        {
            if (!schema.IsValidObject) continue;

            if (schema.SchemaName == Name && schema.GUID != new Guid(Guid))
            {
                foundSchema = schema;
                return true;
            }
        }

        foundSchema = null;
        return false;
    }
    
    public static void Delete()
    {
        var schema = Schema.Lookup(new Guid(Guid));
        if (schema is not null)
        {
            Context.ActiveDocument!.EraseSchemaAndAllEntities(schema);
        }
    }
}