using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AccountBookApp.Infrastructure;
using AccountBookApp.ViewModels;

namespace AccountBookApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        ThemeService.PrepareThemeResources();
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void MainContentScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
        {
            return;
        }

        if (IsInsideIndependentScrollHost(e.OriginalSource as DependencyObject))
        {
            return;
        }

        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3d);
        e.Handled = true;
    }

    private static bool IsInsideIndependentScrollHost(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is DataGrid or ComboBox or DatePicker)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }
}
