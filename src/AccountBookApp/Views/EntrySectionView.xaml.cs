using System.Windows;
using System.Windows.Controls;
using AccountBookApp.Models;
using AccountBookApp.ViewModels;

namespace AccountBookApp.Views;

public partial class EntrySectionView : UserControl
{
    public EntrySectionView()
    {
        InitializeComponent();
    }

    private void InlineEditJournalEntryButton_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetJournalEntryContext(sender, out var viewModel, out var entry))
        {
            viewModel.BeginEditJournalEntry(entry);

            var owner = Window.GetWindow(this);
            var dialog = new EditJournalEntryWindow
            {
                Owner = owner,
                DataContext = viewModel
            };

            dialog.ShowDialog();
        }
    }

    private void InlineDeleteJournalEntryButton_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetJournalEntryContext(sender, out var viewModel, out var entry))
        {
            viewModel.DeleteJournalEntry(entry);
        }
    }

    private void InlineEditAutoTransferButton_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetAutoTransferContext(sender, out var viewModel, out var rule))
        {
            viewModel.BeginEditAutoTransferRule(rule);
        }
    }

    private void InlineDeleteAutoTransferButton_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetAutoTransferContext(sender, out var viewModel, out var rule))
        {
            viewModel.DeleteAutoTransferRule(rule);
        }
    }

    private bool TryGetJournalEntryContext(object sender, out MainViewModel viewModel, out JournalEntry entry)
    {
        viewModel = DataContext as MainViewModel ?? null!;
        entry = (sender as FrameworkElement)?.DataContext as JournalEntry ?? null!;

        return viewModel is not null && entry is not null;
    }

    private bool TryGetAutoTransferContext(object sender, out MainViewModel viewModel, out AutoTransferRule rule)
    {
        viewModel = DataContext as MainViewModel ?? null!;
        rule = (sender as FrameworkElement)?.DataContext as AutoTransferRule ?? null!;

        return viewModel is not null && rule is not null;
    }
}
