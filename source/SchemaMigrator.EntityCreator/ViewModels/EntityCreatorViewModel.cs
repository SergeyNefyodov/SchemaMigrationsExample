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
        EntityMigrator.Migrate(EntryKey.DefaultSchema, EntryKey.MigratedSchema);
    }
}