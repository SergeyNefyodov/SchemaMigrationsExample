using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;
using SchemaMigrator.Database.Core.Models;

namespace SchemaMigrator.Database.Migrations;
public class init_20241007_1003 : Migration
{
    public override Dictionary<string, Guid> GuidDictionary { get; set; } = new Dictionary<string, Guid>()
    {
        {"Persons", new Guid("6a859935-664e-4307-a46e-7c113ed100ba") },
        {"Toys", new Guid("a9ae38e7-c16b-44e3-a959-c6899ee5fe36") },
    };

    public override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddSchemaData(new SchemaBuilderData()
        {
            Guid = GuidDictionary["Persons"],
            Documentation = "Initial schema for Person",
            Name = "Persons",
            VendorId = "Atomatiq"
        },
        new SchemaDescriptor("Persons")
        {
            Fields = new List<FieldDescriptor>()
            {
                new FieldDescriptor( "Id", typeof(Int32) ),
                new FieldDescriptor( "Name", typeof(String) ),
                new FieldDescriptor( "Surname", typeof(String) )
            }
        });

        migrationBuilder.AddSchemaData(new SchemaBuilderData()
        {
            Guid = GuidDictionary["Toys"],
            Documentation = "Initial schema for Toy",
            Name = "Toys",
            VendorId = "Atomatiq"
        },
        new SchemaDescriptor("Toys")
        {
            Fields = new List<FieldDescriptor>()
            {
                new FieldDescriptor( "Name", typeof(String) ),
                new FieldDescriptor( "Type", typeof(String) )
            }
        });

    }

    public override void Down()
    {
        throw new NotSupportedException();
    }
}