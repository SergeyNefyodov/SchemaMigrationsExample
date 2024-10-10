using SchemaMigrations.Abstractions;
using SchemaMigrator.Database.Models;

namespace SchemaMigrationsExample.EntityCreator.Database;

public class ApplicationSchemaContext : SchemaContext
{
    public SchemaSet<Person> Persons { get; set; } = [];
    public SchemaSet<Toy> Toys { get; set; } = [];
}