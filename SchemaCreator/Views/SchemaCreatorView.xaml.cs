using SchemaCreator.ViewModels;

namespace SchemaCreator.Views;

public sealed partial class SchemaCreatorView
{
    public SchemaCreatorView(SchemaCreatorViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}