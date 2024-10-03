namespace SchemaMigrator.Database;

public abstract class Migration
{
    public abstract Guid VersionGuid { get; set; }

    public abstract void Up(MigrationBuilder migrationBuilder);
    public abstract void Down();
}