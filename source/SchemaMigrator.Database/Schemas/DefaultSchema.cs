using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Core;

namespace SchemaMigrator.Database.Schemas;

public class DefaultSchema
{
    private const string Guid = "8BF65FBD-D1A0-4CC2-8842-A2ED5975538F";
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

        builder.AddSimpleField("Id", typeof(int));
        builder.AddSimpleField("Name", typeof(string));
        builder.AddSimpleField("Surname", typeof(string));

        return builder.Finish();
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