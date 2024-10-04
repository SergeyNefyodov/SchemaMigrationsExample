using System.Drawing.Drawing2D;
using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Core.Models;
using SchemaMigrator.Database.Models;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database.Migrations;

public class InitialMigration : Migration
{
    public override Dictionary<string, Guid> GuidDictionary { get; set; } = new Dictionary<string, Guid>()
    {
        { "Persons", new Guid("ABE33466-B4E1-4275-AB71-34493F6BCD08") },
        { "Toys", new Guid("0FF844D7-1E26-4833-A0EC-0E54409FE632") },
    };

    public override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSchema(new SchemaBuilderData()
        {
            Guid = GuidDictionary["Persons"],
            Documentation = "Default schema to update",
            Name = "Persons",
            VendorId = "Atomatiq",
        }, 
            new SchemaDescriptor()
            {
                SchemaName = "Persons",
                Fields = new List<FieldDescriptor>()
                {
                    new FieldDescriptor("Id",typeof(int)),
                    new FieldDescriptor("Name", typeof(string)),
                    new FieldDescriptor("Surname", typeof(string)),
                    new FieldDescriptor("Hobbies",typeof(List<string>)),
                    new FieldDescriptor("Scores", typeof(Dictionary<string, int>)),
                }
        });
    }

    public override void Down()
    {
        throw new NotSupportedException();
    }
}