using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using AccountBookApp.Infrastructure;

namespace AccountBookApp;

public partial class App : Application
{
    private static readonly DispatcherPriority[] CalendarPopupRefreshPriorities =
    [
        DispatcherPriority.Loaded,
        DispatcherPriority.Render,
        DispatcherPriority.ApplicationIdle
    ];

    private static readonly Color CalendarPopupTextFallbackColor = Color.FromRgb(0x17, 0x30, 0x3C);
    private static readonly Color CalendarPopupMutedTextFallbackColor = Color.FromRgb(0x6D, 0x7F, 0x8C);

    protected override void OnStartup(StartupEventArgs e)
    {
        Resources["AppFontFamily"] = ResolveAppFontFamily();
        ThemeService.PrepareThemeResources();
        base.OnStartup(e);
    }

    private static FontFamily ResolveAppFontFamily()
    {
        var preferredFonts = new[]
        {
            "Pretendard Variable",
            "Pretendard",
            "NanumSquare",
            "NanumSquareOTF",
            "NanumSquare Neo",
            "Malgun Gothic"
        };

        foreach (var preferredFont in preferredFonts)
        {
            var matchedFont = Fonts.SystemFontFamilies
                .FirstOrDefault(font => string.Equals(font.Source, preferredFont, StringComparison.OrdinalIgnoreCase));

            if (matchedFont is not null)
            {
                return matchedFont;
            }
        }

        return new FontFamily("Malgun Gothic");
    }

    private void CalendarPopupLoaded(object sender, RoutedEventArgs e)
    {
        QueueCalendarPopupTextRefresh(sender);
    }

    private void CalendarPopupDisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
    {
        QueueCalendarPopupTextRefresh(sender);
    }

    private void CalendarPopupDisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
    {
        QueueCalendarPopupTextRefresh(sender);
    }

    private void DatePickerLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DatePicker datePicker)
        {
            return;
        }

        datePicker.CalendarOpened -= DatePickerCalendarOpened;
        datePicker.CalendarOpened += DatePickerCalendarOpened;
    }

    private void DatePickerCalendarOpened(object sender, RoutedEventArgs e)
    {
        if (sender is DatePicker datePicker
            && datePicker.Template.FindName("PART_Calendar", datePicker) is Calendar calendar)
        {
            QueueCalendarPopupTextRefresh(calendar);
        }
    }

    private static Brush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Brush ResolveBrush(string resourceKey, Color fallbackColor)
    {
        return Application.Current?.TryFindResource(resourceKey) as Brush
            ?? CreateFrozenBrush(fallbackColor);
    }

    private static void QueueCalendarPopupTextRefresh(object sender)
    {
        if (sender is not Calendar calendar)
        {
            return;
        }

        foreach (var priority in CalendarPopupRefreshPriorities)
        {
            calendar.Dispatcher.BeginInvoke(
                new Action(() => ApplyCalendarPopupTextBrushes(calendar)),
                priority);
        }
    }

    private static void ApplyCalendarPopupTextBrushes(DependencyObject root)
    {
        var textBrush = ResolveBrush("CalendarTextBrush", CalendarPopupTextFallbackColor);
        var mutedTextBrush = ResolveBrush("CalendarMutedBrush", CalendarPopupMutedTextFallbackColor);

        ApplyCalendarPopupTextBrushes(root, textBrush, mutedTextBrush);
    }

    private static void ApplyCalendarPopupTextBrushes(
        DependencyObject root,
        Brush textBrush,
        Brush mutedTextBrush)
    {
        if (root is CalendarDayButton dayButton)
        {
            var brush = dayButton.IsInactive ? mutedTextBrush : textBrush;
            dayButton.Foreground = brush;
            dayButton.SetValue(TextElement.ForegroundProperty, brush);
        }
        else if (root is Control control)
        {
            control.Foreground = textBrush;
            control.SetValue(TextElement.ForegroundProperty, textBrush);
        }
        else if (root is TextBlock textBlock)
        {
            textBlock.Foreground = textBrush;
        }
        else if (root is TextElement textElement)
        {
            textElement.Foreground = textBrush;
        }
        else if (root is FrameworkElement element)
        {
            element.SetValue(TextElement.ForegroundProperty, textBrush);
        }

        if (root is Shape shape)
        {
            if (shape.Fill is not null)
            {
                shape.Fill = textBrush;
            }

            if (shape.Stroke is not null)
            {
                shape.Stroke = textBrush;
            }
        }

        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < childCount; index++)
        {
            ApplyCalendarPopupTextBrushes(
                VisualTreeHelper.GetChild(root, index),
                textBrush,
                mutedTextBrush);
        }
    }
}
