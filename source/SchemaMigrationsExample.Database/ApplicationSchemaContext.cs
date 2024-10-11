using SchemaMigrations.Abstractions;
using SchemaMigrationsExample.EntityCreator.Database.Models;

namespace SchemaMigrationsExample.Database;

public class ApplicationSchemaContext : SchemaContext
{
    public SchemaSet<Person> Persons { get; set; } = [];
    //public SchemaSet<Instrument> Instruments { get; set; } = [];
}