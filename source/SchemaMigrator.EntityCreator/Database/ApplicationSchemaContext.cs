using SchemaMigrations.Abstractions;
using SchemaMigrator.Database.Models;

namespace SchemaMigrator.EntityCreator.Database;

public class ApplicationSchemaContext : SchemaContext
{
    public SchemaSet<Person> Persons { get; set; } = [];
    public SchemaSet<Toy> Toys { get; set; } = [];
}