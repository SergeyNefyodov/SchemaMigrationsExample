using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database.Models.Migrations;
        public class secondMigration : Migration
        {
            public override Guid VersionGuid { get; set; } = Guid.NewGuid();

            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.AddColumn("Occupation", typeof(String));
                migrationBuilder.AddColumn("Scores", typeof(Dictionary<String, Int32>));
            }

            public override void Down()
            {
                throw new NotSupportedException();
            }
        }