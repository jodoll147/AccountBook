using System;
using System.Windows;
using AccountBookApp.ViewModels;

namespace AccountBookApp.Views;

public partial class EditJournalEntryWindow : Window
{
    public EditJournalEntryWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel { IsJournalEntryEditMode: false })
        {
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel &&
            viewModel.CancelEntryEditCommand.CanExecute(null))
        {
            viewModel.CancelEntryEditCommand.Execute(null);
        }
    }
}
