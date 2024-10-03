namespace SchemaMigrator.Database.MigrationTool;

public static class MigrationGenerator
{
    public static void GenerateInitialMigration(string migrationName, Type schemaSetType)
    {
        var properties = schemaSetType.GetProperties()
            .ToDictionary(prop => prop.Name, prop => prop.PropertyType);

        var migrationCode = $@"
        public class {migrationName} : Migration
        {{
            public override Guid VersionGuid {{ get; set; }} = Guid.NewGuid();

            public override void Up(MigrationBuilder migrationBuilder)
            {{
                migrationBuilder.CreateSchema(new SchemaBuilderData()
                {{
                    Guid = VersionGuid,
                    Documentation = ""Initial schema for {schemaSetType.Name}"",
                    Name = ""{schemaSetType.Name}"",
                    VendorId = ""Me""
                }},
                new Dictionary<string, Type>()
                {{
                    {string.Join(",\n                    ", properties.Select(p => $"{{ \"{p.Key}\", typeof({p.Value.Name}) }}"))}
                }});
            }}

            public override void Down()
            {{
                throw new NotSupportedException();
            }}
        }}";
        SaveFile(migrationName, migrationCode);
    }

    public static void GenerateMigration(string migrationName, List<string> changes)
    {
        string migrationCode = $@"
        public class {migrationName} : Migration
        {{
            public override Guid VersionGuid {{ get; set; }} = Guid.NewGuid();

            public override void Up(MigrationBuilder migrationBuilder)
            {{
                {string.Join(Environment.NewLine + "                ", changes.Select(change => $"migrationBuilder.{change};"))}
            }}

            public override void Down()
            {{
                throw new NotSupportedException();
            }}
        }}";

        SaveFile(migrationName, migrationCode);
    }
    
    private static void SaveFile(string migrationName, string migrationCode)
    {
        var migrationsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Migrations");

        if (!Directory.Exists(migrationsFolderPath))
        {
            Directory.CreateDirectory(migrationsFolderPath);
        }

        var filePath = Path.Combine(migrationsFolderPath, $"{DateTime.Now:YYYYMMDD_hhmm}_{migrationName}.cs");
        File.WriteAllText(filePath, migrationCode);
    }
}