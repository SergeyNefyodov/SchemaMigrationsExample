using Autodesk.Revit.UI;
using SchemaMigrations.Database;
using SchemaMigrationsExample.EntityCreator.Database.Models;
using SchemaMigrationsExample.EntityCreator.Models;

namespace SchemaMigrationsExample.EntityCreator.ViewModels;

public sealed partial class EntityCreatorViewModel : ObservableObject
{
    [RelayCommand]
    private void SeedSchema()
    {
        EntitySeeder.Seed();
    }
    
    [RelayCommand]
    private void ReadData()
    {
        var element = Context.ActiveDocument.EnumerateInstances<FamilyInstance>().First();
        var connection = new DatabaseConnection<Person>(element);
        var person = connection.LoadObject();
        TaskDialog.Show("Read Data", person.ToString());
    }
}