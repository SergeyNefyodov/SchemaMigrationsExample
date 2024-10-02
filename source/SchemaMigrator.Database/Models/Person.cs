namespace SchemaMigrator.Database.Models;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public List<string> Hobbies { get; set; }
    public Dictionary<string, int> Scores { get; set; }
}