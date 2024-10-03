using SchemaMigrator.Database;
using SchemaMigrator.Database.Enums;
using SchemaMigrator.Database.Models;
using SchemaMigrator.EntityCreator.Models;

namespace SchemaMigrator.EntityCreator.ViewModels;

public sealed partial class EntityCreatorViewModel : ObservableObject
{
    [RelayCommand]
    private void SeedSchema()
    {
        EntitySeeder.Seed(EntryKey.DefaultSchema);
    }

    [RelayCommand]
    private void MigrateSchema()
    {
        EntitySeeder.Seed(EntryKey.MigratedSchema);
    }
    
    [RelayCommand]
    private void DeleteSchemas()
    {
        var connection = new DatabaseConnection<Person>(null);
        connection.Delete();
    }
    
    [RelayCommand]
    private void ReadData()
    {
        var element = Context.ActiveDocument.EnumerateInstances<FamilyInstance>().First();
        var connection = new DatabaseConnection<Person>(element);
        var person = connection.LoadObject();
    }
}