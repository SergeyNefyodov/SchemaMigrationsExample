using Nice3point.Revit.Toolkit.External;
using SchemaMigrator.Application.Commands;

namespace SchemaMigrator.Application;

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
        var panel = Application.CreatePanel("Commands", "SchemaMigrator.Application");

        panel.AddPushButton<StartupCommand>("Execute")
            .SetImage("/SchemaMigrator.Application;component/Resources/Icons/RibbonIcon16.png")
            .SetLargeImage("/SchemaMigrator.Application;component/Resources/Icons/RibbonIcon32.png");
    }
}