using Autodesk.Revit.DB.ExtensibleStorage;

namespace SchemaMigrator.Database.Schemas;

public class MigratedSchema
{
    public static Schema Create()
    {
        var guid = new Guid("7FBF03FC-8F85-4008-BC97-DD3461CE517A");
        var schema = Schema.Lookup(guid);
        if (schema is not null) return schema;

        var builder = new SchemaBuilder(guid)
            .SetSchemaName("DefaultSchema")
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
}