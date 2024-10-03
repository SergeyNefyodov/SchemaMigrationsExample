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
        DatabaseConnection.Delete(EntryKey.DefaultSchema);
        DatabaseConnection.Delete(EntryKey.MigratedSchema);
    }
    
    [RelayCommand]
    private void ReadData()
    {
        var element = Context.ActiveDocument.EnumerateInstances<FamilyInstance>().First();
        var connection = new DatabaseConnection(element, EntryKey.DefaultSchema);
        var person = connection.LoadObject<Person>();
    }
}