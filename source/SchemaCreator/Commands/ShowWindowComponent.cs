using SchemaCreator.Views;

namespace SchemaCreator.Commands;

/// <summary>
///     Command entry point invoked from the Revit AddIn Application
/// </summary>
public class ShowWindowComponent(SchemaCreatorView view)
{
    public void Execute()
    {
        view.ShowDialog();
    }
}