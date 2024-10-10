using Nice3point.Revit.Toolkit.External;
using SchemaMigrationsExample.Application.Commands;

namespace SchemaMigrationsExample.Application;

/// <summary>
///     Application entry point
/// </summary>
[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        Host.Start();
        CreateRibbon();
    }

    public override void OnShutdown()
    {
        Host.Stop();
    }

    private void CreateRibbon()
    {
        var panel = Application.CreatePanel("Commands", "SchemaMigrationsExample");

        panel.AddPushButton<EntityCreatorCommand>("Execute")
            .SetImage("/SchemaMigrationsExample.Application;component/Resources/Icons/RibbonIcon16.png")
            .SetLargeImage("/SchemaMigrationsExample.Application;component/Resources/Icons/RibbonIcon32.png");
    }
}