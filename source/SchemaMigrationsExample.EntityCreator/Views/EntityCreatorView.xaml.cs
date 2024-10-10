using SchemaMigrationsExample.EntityCreator.ViewModels;

namespace SchemaMigrationsExample.EntityCreator.Views;

public sealed partial class EntityCreatorView
{
    public EntityCreatorView(EntityCreatorViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}