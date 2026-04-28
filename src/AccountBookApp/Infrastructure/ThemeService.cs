using System.Windows;
using System.Windows.Media;
using AccountBookApp.Models;

namespace AccountBookApp.Infrastructure;

public static class ThemeService
{
    private static readonly string[] ThemeBrushKeys =
    [
        "AppBackgroundBrush",
        "PaperBackgroundBrush",
        "PanelBackgroundBrush",
        "PanelRaisedBrush",
        "SidebarBackgroundBrush",
        "HeaderBackgroundBrush",
        "HeaderMutedBrush",
        "TextPrimaryBrush",
        "TextSecondaryBrush",
        "TextMutedBrush",
        "AccentBrush",
        "AccentTextBrush",
        "AccentStrongBrush",
        "PositiveValueBrush",
        "NegativeValueBrush",
        "ChipBackgroundBrush",
        "InputBackgroundBrush",
        "InputSurfaceBrush",
        "InputContrastTextBrush",
        "CalendarSurfaceBrush",
        "CalendarTextBrush",
        "CalendarMutedBrush",
        "LineBrush",
        "HoverBrush",
        "WarningTextBrush",
        "WarningBackgroundBrush",
        "WarningBorderBrush",
        "InfoBackgroundBrush",
        "SecondaryButtonBrush",
        "SecondaryButtonBorderBrush",
        "SecondaryButtonTextBrush",
        "TabIdleBackgroundBrush",
        "TabActiveBackgroundBrush",
        "ChartGridBrush",
        "NetAssetLineBrush",
        "ExpenseLineBrush"
    ];

    public static void PrepareThemeResources()
    {
        var resources = Application.Current.Resources;

        foreach (var key in ThemeBrushKeys)
        {
            if (resources[key] is SolidColorBrush brush)
            {
                resources[key] = CreateBrush(brush.Color);
            }
        }
    }

    public static void ApplyTheme(AppThemeMode themeMode)
    {
        PrepareThemeResources();

        var resources = Application.Current.Resources;
        var palette = themeMode switch
        {
            AppThemeMode.Light => CreateLightPalette(),
            AppThemeMode.Rose => CreateRosePalette(),
            _ => CreateDarkPalette()
        };

        foreach (var pair in palette)
        {
            ApplyBrush(resources, pair.Key, pair.Value);
        }
    }

    private static void ApplyBrush(ResourceDictionary resources, string key, Color color)
    {
        resources[key] = CreateBrush(color);
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        return new SolidColorBrush(color);
    }

    private static Dictionary<string, Color> CreateDarkPalette()
    {
        return new Dictionary<string, Color>
        {
            ["AppBackgroundBrush"] = Color("#0A0F14"),
            ["PaperBackgroundBrush"] = Color("#111820"),
            ["PanelBackgroundBrush"] = Color("#151F28"),
            ["PanelRaisedBrush"] = Color("#1A2631"),
            ["SidebarBackgroundBrush"] = Color("#0D141B"),
            ["HeaderBackgroundBrush"] = Color("#1587A8"),
            ["HeaderMutedBrush"] = Color("#0E6C88"),
            ["TextPrimaryBrush"] = Color("#EEF5F7"),
            ["TextSecondaryBrush"] = Color("#B8C9D4"),
            ["TextMutedBrush"] = Color("#8AA0AE"),
            ["AccentBrush"] = Color("#F0A83A"),
            ["AccentTextBrush"] = Color("#0A0F14"),
            ["AccentStrongBrush"] = Color("#FFC96B"),
            ["PositiveValueBrush"] = Color("#FF7A72"),
            ["NegativeValueBrush"] = Color("#5DAEFF"),
            ["ChipBackgroundBrush"] = Color("#21313E"),
            ["InputBackgroundBrush"] = Color("#0D151C"),
            ["InputSurfaceBrush"] = Color("#F3F7FA"),
            ["InputContrastTextBrush"] = Color("#16303C"),
            ["CalendarSurfaceBrush"] = Color("#F8FBFD"),
            ["CalendarTextBrush"] = Color("#17303C"),
            ["CalendarMutedBrush"] = Color("#738696"),
            ["LineBrush"] = Color("#24333E"),
            ["HoverBrush"] = Color("#1A2632"),
            ["WarningTextBrush"] = Color("#F6C1B3"),
            ["WarningBackgroundBrush"] = Color("#3B2021"),
            ["WarningBorderBrush"] = Color("#70403C"),
            ["InfoBackgroundBrush"] = Color("#12212A"),
            ["SecondaryButtonBrush"] = Color("#202E39"),
            ["SecondaryButtonBorderBrush"] = Color("#314250"),
            ["SecondaryButtonTextBrush"] = Color("#EAF3F7"),
            ["TabIdleBackgroundBrush"] = Color("#131C24"),
            ["TabActiveBackgroundBrush"] = Color("#1A2732"),
            ["ChartGridBrush"] = Color("#2B3C48"),
            ["NetAssetLineBrush"] = Color("#52D0B2"),
            ["ExpenseLineBrush"] = Color("#F4A261")
        };
    }

    private static Dictionary<string, Color> CreateLightPalette()
    {
        return new Dictionary<string, Color>
        {
            ["AppBackgroundBrush"] = Color("#F5F7FA"),
            ["PaperBackgroundBrush"] = Color("#FFFFFF"),
            ["PanelBackgroundBrush"] = Color("#F8FBFD"),
            ["PanelRaisedBrush"] = Color("#EEF4F7"),
            ["SidebarBackgroundBrush"] = Color("#EEF3F7"),
            ["HeaderBackgroundBrush"] = Color("#5FA4D8"),
            ["HeaderMutedBrush"] = Color("#DCEBF6"),
            ["TextPrimaryBrush"] = Color("#17303C"),
            ["TextSecondaryBrush"] = Color("#5F7380"),
            ["TextMutedBrush"] = Color("#7D909C"),
            ["AccentBrush"] = Color("#4D9BF5"),
            ["AccentTextBrush"] = Color("#FFFFFF"),
            ["AccentStrongBrush"] = Color("#2E79D6"),
            ["PositiveValueBrush"] = Color("#D9534F"),
            ["NegativeValueBrush"] = Color("#2F7FE0"),
            ["ChipBackgroundBrush"] = Color("#E3EEF3"),
            ["InputBackgroundBrush"] = Color("#FFFFFF"),
            ["InputSurfaceBrush"] = Color("#FFFFFF"),
            ["InputContrastTextBrush"] = Color("#17303C"),
            ["CalendarSurfaceBrush"] = Color("#FFFFFF"),
            ["CalendarTextBrush"] = Color("#17303C"),
            ["CalendarMutedBrush"] = Color("#7E909D"),
            ["LineBrush"] = Color("#D2DEE5"),
            ["HoverBrush"] = Color("#EAF2F6"),
            ["WarningTextBrush"] = Color("#9D4C43"),
            ["WarningBackgroundBrush"] = Color("#F9EAE6"),
            ["WarningBorderBrush"] = Color("#E7C9C2"),
            ["InfoBackgroundBrush"] = Color("#F1F7FA"),
            ["SecondaryButtonBrush"] = Color("#E4EDF2"),
            ["SecondaryButtonBorderBrush"] = Color("#C9D7DF"),
            ["SecondaryButtonTextBrush"] = Color("#17303C"),
            ["TabIdleBackgroundBrush"] = Color("#E8EFF3"),
            ["TabActiveBackgroundBrush"] = Color("#FFFFFF"),
            ["ChartGridBrush"] = Color("#D7E2E8"),
            ["NetAssetLineBrush"] = Color("#2D8C74"),
            ["ExpenseLineBrush"] = Color("#D8884A")
        };
    }

    private static Dictionary<string, Color> CreateRosePalette()
    {
        return new Dictionary<string, Color>
        {
            ["AppBackgroundBrush"] = Color("#FFF6FA"),
            ["PaperBackgroundBrush"] = Color("#FFFDFE"),
            ["PanelBackgroundBrush"] = Color("#FFF7FB"),
            ["PanelRaisedBrush"] = Color("#FCECF4"),
            ["SidebarBackgroundBrush"] = Color("#FCEAF2"),
            ["HeaderBackgroundBrush"] = Color("#F39AB7"),
            ["HeaderMutedBrush"] = Color("#FCE0EA"),
            ["TextPrimaryBrush"] = Color("#4B2C3C"),
            ["TextSecondaryBrush"] = Color("#876478"),
            ["TextMutedBrush"] = Color("#A88A9A"),
            ["AccentBrush"] = Color("#F07FA8"),
            ["AccentTextBrush"] = Color("#4B2C3C"),
            ["AccentStrongBrush"] = Color("#D85C89"),
            ["PositiveValueBrush"] = Color("#D64545"),
            ["NegativeValueBrush"] = Color("#4B8BDB"),
            ["ChipBackgroundBrush"] = Color("#F9E3EC"),
            ["InputBackgroundBrush"] = Color("#FFFDFE"),
            ["InputSurfaceBrush"] = Color("#FFFDFE"),
            ["InputContrastTextBrush"] = Color("#4B2C3C"),
            ["CalendarSurfaceBrush"] = Color("#FFFDFE"),
            ["CalendarTextBrush"] = Color("#4B2C3C"),
            ["CalendarMutedBrush"] = Color("#907182"),
            ["LineBrush"] = Color("#F0D5E0"),
            ["HoverBrush"] = Color("#FBEAF1"),
            ["WarningTextBrush"] = Color("#9D4C43"),
            ["WarningBackgroundBrush"] = Color("#FCEBEC"),
            ["WarningBorderBrush"] = Color("#E8C7CD"),
            ["InfoBackgroundBrush"] = Color("#FDF1F6"),
            ["SecondaryButtonBrush"] = Color("#F6E2EB"),
            ["SecondaryButtonBorderBrush"] = Color("#E6C3D2"),
            ["SecondaryButtonTextBrush"] = Color("#4B2C3C"),
            ["TabIdleBackgroundBrush"] = Color("#F8E4EC"),
            ["TabActiveBackgroundBrush"] = Color("#FFFDFE"),
            ["ChartGridBrush"] = Color("#E8D2DC"),
            ["NetAssetLineBrush"] = Color("#D86F9B"),
            ["ExpenseLineBrush"] = Color("#F2A65A")
        };
    }

    private static Color Color(string hex)
    {
        return (Color)ColorConverter.ConvertFromString(hex);
    }
}
