using SchemaMigrationsExample.EntityCreator.Views;

namespace SchemaMigrationsExample.EntityCreator.Commands;

/// <summary>
///     Command entry point invoked from the Revit AddIn Application
/// </summary>
public class ShowEntityCreatorComponent(EntityCreatorView view)
{
    public void Execute()
    {
        view.ShowDialog();
    }
}