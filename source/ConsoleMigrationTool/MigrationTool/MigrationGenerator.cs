using System.IO;
using System.Reflection;
using System.Text;

namespace ConsoleMigrationTool.MigrationTool;

public class MigrationGenerator(Type modelType)
{
    private StringBuilder _upBuilder = new();
    private StringBuilder _guidsBuilder = new();
    private readonly string _projectName = modelType.Namespace!.Remove(modelType.Namespace.LastIndexOf('.'));

    public void Finish(string migrationName)
    {
        var migrationCode = $@"using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Schemas;
using SchemaMigrator.Database.Core.Models;

namespace {_projectName}.Migrations;
public class {migrationName} : Migration
{{
    public override Dictionary<string, Guid> GuidDictionary {{ get; set; }} = new Dictionary<string, Guid>()
    {{
{_guidsBuilder}    }};

    public override void Up(MigrationBuilder migrationBuilder)
    {{
{_upBuilder}    }}

    public override void Down()
    {{
        throw new NotSupportedException();
    }}
}}";

        SaveFile(migrationName, migrationCode);
    }

    public void AddInitialMigration(string schemaName, Type schemaSetType)
    {
        var properties = schemaSetType.GetProperties()
            .ToDictionary(prop => prop.Name, prop => prop.PropertyType);

        _guidsBuilder.Append($@"        {{""{schemaName}"", new Guid(""{Guid.NewGuid()}"") }},
");
        _upBuilder.Append($@"        migrationBuilder.CreateSchema(new SchemaBuilderData()
        {{
            Guid = GuidDictionary[""{schemaName}""],
            Documentation = ""Initial schema for {schemaSetType.Name}"",
            Name = ""{schemaSetType.Name}"",
            VendorId = ""Atomatiq""
        }},
        new SchemaDescriptor(""{schemaSetType.Name}"")
        {{
            Fields = new List<FieldDescriptor>()
            {{
                {string.Join(",\n                ", properties.Select(p =>
                {
                    var type = p.Value;
                    if (type.IsGenericType)
                    {
                        var genericTypeName = type.GetGenericTypeDefinition().Name;
                        var genericArguments = string.Join(", ", type.GetGenericArguments().Select(t => t.Name));
                        var fullTypeName = $"{genericTypeName.Substring(0, genericTypeName.IndexOf('`'))}<{genericArguments}>";
                        return $"new FieldDescriptor( \"{p.Key}\", typeof({fullTypeName}) )";
                    }

                    return $"new FieldDescriptor( \"{p.Key}\", typeof({type.Name}) )";
                }))}
            }}
        }});

");
    }

    public void AddMigration(string schemaName, List<string> changes, Type schemaSetType)
    {
        _guidsBuilder.Append($@"        {{""{schemaName}"", new Guid(""{Guid.NewGuid()}"") }},
");
        _upBuilder.AppendLine(
            $$"""{ "{{string.Join(Environment.NewLine + "                ", changes.Select(change => $"migrationBuilder.{change};"))}}") },""");
    }

    private void SaveFile(string migrationName, string migrationCode)
    {
        var solutionDirectory = GetSolutionDirectory(modelType.Assembly);
        var projectDirectory = FindProjectDirectory(solutionDirectory, modelType.Name);
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