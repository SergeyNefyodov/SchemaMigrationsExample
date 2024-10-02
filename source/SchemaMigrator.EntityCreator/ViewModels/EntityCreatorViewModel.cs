using SchemaMigrator.Database;
using SchemaMigrator.Database.Core;
using SchemaMigrator.Database.Enums;
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
}