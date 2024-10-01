using Autodesk.Revit.DB.ExtensibleStorage;

namespace SchemaMigrator.Database.Schemas;

public class DefaultSchema
{
    public static Schema Create()
    {
        var guid = new Guid("8BF65FBD-D1A0-4CC2-8842-A2ED5975538F");
        var schema = Schema.Lookup(guid);
        if (schema is not null) return schema;

        var builder = new SchemaBuilder(guid)
            .SetSchemaName("DefaultSchema")
            .SetDocumentation("Default schema to be updated")
            .SetVendorId("Atomatiq")
            .SetReadAccessLevel(AccessLevel.Public)
            .SetWriteAccessLevel(AccessLevel.Public);

        builder.AddSimpleField("Id", typeof(int));
        builder.AddSimpleField("Name", typeof(string));
        builder.AddSimpleField("Surname", typeof(string));

        return builder.Finish();
    }
}