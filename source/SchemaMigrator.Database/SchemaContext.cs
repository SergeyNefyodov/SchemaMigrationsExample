using SchemaMigrator.Database.Models;

namespace SchemaMigrator.Database;

public class SchemaContext
{
    public SchemaSet<Person> Persons { get; set; } = [];
}