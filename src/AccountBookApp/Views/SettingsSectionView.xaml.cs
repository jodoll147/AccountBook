using System.Windows;
using System.Windows.Controls;
using AccountBookApp.Models;
using AccountBookApp.ViewModels;

namespace AccountBookApp.Views;

public partial class SettingsSectionView : UserControl
{
    public SettingsSectionView()
    {
        InitializeComponent();
    }

    private void AccountCard_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel || sender is not FrameworkElement element)
        {
            return;
        }

        viewModel.SetCurrentManagedAccount(element.DataContext as AccountDefinition);
    }

    private void EditAccountMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.OpenEditCurrentAccount();
        }
    }

    private void InlineEditAccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (TrySetCurrentManagedAccount(sender, out var viewModel))
        {
            viewModel.OpenEditCurrentAccount();
        }
    }

    private void ArchiveAccountMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.DeleteCurrentAccount();
        }
    }

    private void InlineDeleteAccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (TrySetCurrentManagedAccount(sender, out var viewModel))
        {
            viewModel.DeleteCurrentAccount();
        }
    }

    private bool TrySetCurrentManagedAccount(object sender, out MainViewModel viewModel)
    {
        viewModel = DataContext as MainViewModel ?? null!;
        if (viewModel is null || sender is not FrameworkElement element)
        {
            return false;
        }

        viewModel.SetCurrentManagedAccount(element.DataContext as AccountDefinition);
        return true;
    }
}
