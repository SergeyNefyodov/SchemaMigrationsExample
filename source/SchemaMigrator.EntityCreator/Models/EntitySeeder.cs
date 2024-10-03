using Bogus;
using SchemaMigrator.Database;
using SchemaMigrator.Database.Enums;
using Person = SchemaMigrator.Database.Models.Person;

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
            var connection = new DatabaseConnection<Person>(instance);
            var person = new Person
            {
                Id = faker.Random.Int(1, 1000),
                Name = faker.Person.FirstName,
                Surname = faker.Person.LastName,
            };
            person.Scores.Add("Score", 1);
            person.Hobbies.Add("Hobbyhorsing");
            connection.SaveObject(person);
        }

        transaction.Commit();
    }
}