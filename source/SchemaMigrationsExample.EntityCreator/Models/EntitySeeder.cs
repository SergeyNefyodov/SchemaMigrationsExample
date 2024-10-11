using SchemaMigrations.Database;
using Person = SchemaMigrationsExample.EntityCreator.Database.Models.Person;

namespace SchemaMigrationsExample.EntityCreator.Models;

public static class EntitySeeder
{
    public static void Seed()
    {
        var instances = Context.ActiveDocument.EnumerateInstances<FamilyInstance>().ToArray();
        Element instance = null;
        using var transaction = new Transaction(instance.Document, "Seeding database");
        transaction.Start();
        
        var connection = new DatabaseConnection<Person>(instance);
        var person = new Person
        {
            Id = 69,
            Name = "Branimir",
            Surname = "Petrović",
            //Occupation = "Software development",
            Scores = new Dictionary<string, int>
            {
                { "Scores", 1 }
            },
            Hobbies = ["Sports"]
        };
        connection.SaveObject(person);

        transaction.Commit();

        var existedPerson = connection.LoadObject();

        // var toyConnection = new DatabaseConnection<Toy>(instance);
        // var toy = new Toy()
        // {
        //     Name = faker.Person.FirstName,
        //     Type = faker.Database.Type()
        // };
        // toyConnection.SaveObject(toy);

        transaction.Commit();
    }
}