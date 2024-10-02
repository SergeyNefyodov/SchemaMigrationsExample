using Bogus;
using SchemaMigrator.Database;
using SchemaMigrator.Database.Enums;

namespace SchemaMigrator.EntityCreator.Models;

public static class EntitySeeder
{
    public static void Seed(EntryKey entryKey)
    {
        var instances = Context.ActiveDocument.EnumerateInstances<FamilyInstance>().ToArray();
        using var transaction = new Transaction(Context.ActiveDocument, "Seeding database");
        transaction.Start();
        var faker = new Faker();
        foreach (var instance in instances)
        {
            var connection = new DatabaseConnection(instance, entryKey);
            if (entryKey == EntryKey.DefaultSchema)
            {
                connection.Save("Id", faker.Random.Int(0, 999));
                connection.Save("Name", faker.Person.FirstName);
                connection.Save("Surname", faker.Person.LastName);
            }

            if (entryKey == EntryKey.MigratedSchema)
            {
                connection.Save("Vehicle", faker.Vehicle.Model());
            }
        }
        transaction.Commit();
    }
    
}