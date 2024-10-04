using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database.Models.Migrations;
        public class NewMigration : Migration
        {
            public override Guid VersionGuid { get; set; } = Guid.NewGuid();

            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateSchema(new SchemaBuilderData()
                {
                    Guid = VersionGuid,
                    Documentation = "Initial schema for Person",
                    Name = "Person",
                    VendorId = "Atomatiq"
                },
                new Dictionary<string, Type>()
                {
                    { "Id", typeof(Int32) },
                    { "Name", typeof(String) },
                    { "Surname", typeof(String) },
                    { "Hobbies", typeof(List<String>) }
                });
            }

            public override void Down()
            {
                throw new NotSupportedException();
            }
        }