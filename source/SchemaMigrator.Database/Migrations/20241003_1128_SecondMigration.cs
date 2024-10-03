using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database.Migrations;

public class SecondMigration : Migration
{
    public override Guid VersionGuid { get; set; } = new Guid("B08ABEC8-1A79-4860-8EB5-824B0FE4970E");

    public override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateGuid("Persons", VersionGuid);
        migrationBuilder.AddColumn("Occupation", typeof(string));
    }

    public override void Down()
    {
        throw new NotSupportedException();
    }
}