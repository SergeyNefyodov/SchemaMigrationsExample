using System.IO;
using System.Reflection;

namespace ConsoleMigrationTool.MigrationTool;

public static class MigrationGenerator
{
    public static void GenerateInitialMigration(string migrationName, Type schemaSetType)
    {
        var properties = schemaSetType.GetProperties()
            .ToDictionary(prop => prop.Name, prop => prop.PropertyType);

        var migrationCode = $@"using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;

namespace {schemaSetType.Namespace}.Migrations;
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
                    VendorId = ""Atomatiq""
                }},
                new Dictionary<string, Type>()
                {{
                    {string.Join(",\n                    ", properties.Select(p =>
                    {
                        var type = p.Value;
                        if (type.IsGenericType)
                        {
                            var genericTypeName = type.GetGenericTypeDefinition().Name;
                            var genericArguments = string.Join(", ", type.GetGenericArguments().Select(t => t.Name));
                            var fullTypeName = $"{genericTypeName.Substring(0, genericTypeName.IndexOf('`'))}<{genericArguments}>";
                            return $"{{ \"{p.Key}\", typeof({fullTypeName}) }}";
                        }
                        else
                        {
                            return $"{{ \"{p.Key}\", typeof({type.Name}) }}";
                        }
                    }))}
                }});
            }}

            public override void Down()
            {{
                throw new NotSupportedException();
            }}
        }}";
        SaveFile(migrationName, migrationCode, schemaSetType);
    }

    public static void GenerateMigration(string migrationName, List<string> changes, Type schemaSetType)
    {
        var migrationCode = $@"using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;

namespace {schemaSetType.Namespace}.Migrations;
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

        SaveFile(migrationName, migrationCode, schemaSetType);
    }
    
    private static void SaveFile(string migrationName, string migrationCode, Type type)
    {
        var assembly = type.Assembly;

        var solutionDirectory = GetSolutionDirectory(assembly);
        var projectDirectory = FindProjectDirectory(solutionDirectory, type.Name);
        var migrationsFolderPath = Path.Combine(projectDirectory, "Migrations");

        if (!Directory.Exists(migrationsFolderPath))
        {
            Directory.CreateDirectory(migrationsFolderPath);
        }

        var filePath = Path.Combine(migrationsFolderPath, $"{DateTime.Now:yyyyMMdd_hhmm}_{migrationName}.cs");
        File.WriteAllText(filePath, migrationCode);
    }
    
    private static string GetSolutionDirectory(Assembly assembly)
    {
        var currentDir = new DirectoryInfo(Path.GetDirectoryName(assembly.Location)!);

        while (currentDir != null && !currentDir.GetFiles("*.sln").Any())
        {
            currentDir = currentDir.Parent;
        }

        return currentDir?.FullName;
    }

    private static string FindProjectDirectory(string solutionDirectory, string className)
    {
        if (string.IsNullOrEmpty(solutionDirectory)) return string.Empty;

        var directories = Directory.GetDirectories(solutionDirectory, "*", SearchOption.AllDirectories);
        
        foreach (var dir in directories)
        {
            if (Directory.GetFiles(dir, "*.csproj").Any())
            {
                var files = Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories);
                if (files.Any(f => File.ReadAllText(f).Contains($"public class {className}")))
                {
                    return dir;
                }
            }
        }

        return string.Empty;
    }
}