using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using SchemaMigrationsExample.EntityCreator.Commands;

namespace SchemaMigrationsExample.Application.Commands;

/// <summary>
///     External command entry point invoked from the Revit interface
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class EntityCreatorCommand : ExternalCommand
{
    public override void Execute()
    {
        var scopeFactory = Host.GetService<IServiceScopeFactory>();
        using var serviceScope = scopeFactory.CreateScope();
        serviceScope.ServiceProvider.GetRequiredService<ShowEntityCreatorComponent>().Execute();
    }
}