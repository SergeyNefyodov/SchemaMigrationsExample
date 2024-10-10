using Bogus;
using SchemaMigrations.Database;
using SchemaMigrator.Database.Models;
using Person = SchemaMigrator.Database.Models.Person;

namespace SchemaMigrator.EntityCreator.Models;

public static class EntitySeeder
{
    public static void Seed()
    {
        var instances = Context.ActiveDocument.EnumerateInstances<FamilyInstance>().ToArray();
        using var transaction = new Transaction(Context.ActiveDocument, "Seeding database");
        transaction.Start();
        var faker = new Faker();
        foreach (var instance in instances)
        {
            var connection = new DatabaseConnection<Person>(instance);
            var person = connection.LoadObject();
            //var person = new Person
            // {
            //     Id = faker.Random.Int(1, 1000),
            //     Name = faker.Person.FirstName,
            //     Surname = faker.Person.LastName,
            //     Occupation = "Software development",
            // };
            person.Occupation = "Software development";
            person.Scores.Add("Score", 1);
            person.Hobbies.Add("Hobbyhorsing");
            connection.SaveObject(person);

            var toyConnection = new DatabaseConnection<Toy>(instance);
            var toy = new Toy()
            {
                Name = faker.Person.FirstName,
                Type = faker.Database.Type()
            };
            toyConnection.SaveObject(toy);
        }

        transaction.Commit();
    }
}