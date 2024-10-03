using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database.Migrations;

public class InitialMigration : Migration
{
    public override Guid VersionGuid { get; set; } = new Guid("ABE33466-B4E1-4275-AB71-34493F6BCD08");

    public override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSchema(new SchemaBuilderData()
        {
            Guid = VersionGuid,
            Documentation = "Default schema to update",
            Name = "Persons",
            VendorId = "Atomatiq",
        }, 
            new Dictionary<string, Type>()
            {
                { "Id", typeof(int) },
                { "Name", typeof(string) },
                { "Surname", typeof(string) },
                { "Hobbies", typeof(List<string>) },
                { "Scores", typeof(Dictionary<string, int>) },
        });
    }

    public override void Down()
    {
        throw new NotSupportedException();
    }
}