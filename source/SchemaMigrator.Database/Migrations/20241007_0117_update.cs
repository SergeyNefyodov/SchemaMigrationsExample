using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;
using SchemaMigrator.Database.Core.Models;

namespace SchemaMigrator.Database.Migrations;
public class update : Migration
{
    public override Dictionary<string, Guid> GuidDictionary { get; set; } = new Dictionary<string, Guid>()
    {
        { "Persons", new Guid("48b6616c-2513-4754-bc4a-e5ec914730a8") },
    };

    public override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateGuid("Persons", GuidDictionary["Persons"]);
        migrationBuilder.AddColumn("Persons", "Occupation", typeof(String));
        migrationBuilder.AddColumn("Persons", "Hobbies", typeof(List<String>));
        migrationBuilder.AddColumn("Persons", "Scores", typeof(Dictionary<String, Int32>));
    }

    public override void Down()
    {
        throw new NotSupportedException();
    }
}