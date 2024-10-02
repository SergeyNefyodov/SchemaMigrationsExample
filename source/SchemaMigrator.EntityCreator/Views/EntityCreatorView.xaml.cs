using SchemaMigrator.EntityCreator.ViewModels;

namespace SchemaMigrator.EntityCreator.Views;

public sealed partial class EntityCreatorView
{
    public EntityCreatorView(EntityCreatorViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}