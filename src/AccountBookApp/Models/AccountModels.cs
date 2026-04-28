using System.Windows.Media;
using AccountBookApp.Infrastructure;

namespace AccountBookApp.Models;

public enum AccountType
{
    Asset,
    Investment,
    Liability,
    Equity,
    Revenue,
    Expense
}

public enum AppThemeMode
{
    Dark,
    Light,
    Rose
}

public enum AnalysisTab
{
    NetAsset,
    Expense,
    AccountStatus
}

public enum AccountStatusPeriodPreset
{
    OneMonth,
    ThreeMonths,
    SixMonths,
    OneYear,
    All
}

public enum JournalEntryKind
{
    Expense,
    Transfer,
    Revenue
}

public sealed class AccountDefinition : ObservableObject
{
    private string _code;
    private string _name;
    private string _description;
    private AccountType _type;
    private string _icon;
    private bool _isArchived;

    public AccountDefinition(string code, string name, string description, AccountType type, string icon)
    {
        _code = code;
        _name = name;
        _description = description;
        _type = type;
        _icon = icon;
        _isArchived = false;
    }

    public string Code
    {
        get => _code;
        set
        {
            if (SetProperty(ref _code, value))
            {
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(TypeLabel));
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public AccountType Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, value))
            {
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(TypeLabel));
            }
        }
    }

    public string Icon
    {
        get => _icon;
        set
        {
            if (SetProperty(ref _icon, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public bool IsArchived
    {
        get => _isArchived;
        set => SetProperty(ref _isArchived, value);
    }

    public string TypeLabel => Type.ToKoreanLabel();

    public string DisplayName => $"{Icon} {Name} · {TypeLabel}";
}

public sealed class JournalEntry : ObservableObject
{
    private DateTime _entryDate;
    private string _description;
    private string _toAccountCode;
    private string _toAccountName;
    private string _toAccountIcon;
    private string _fromAccountCode;
    private string _fromAccountName;
    private string _fromAccountIcon;
    private decimal _amount;
    private string _memo;
    private JournalEntryKind _transactionKind = JournalEntryKind.Transfer;

    public JournalEntry(
        DateTime entryDate,
        string description,
        string toAccountCode,
        string toAccountName,
        string toAccountIcon,
        string fromAccountCode,
        string fromAccountName,
        string fromAccountIcon,
        decimal amount,
        string memo)
    {
        _entryDate = entryDate;
        _description = description;
        _toAccountCode = toAccountCode;
        _toAccountName = toAccountName;
        _toAccountIcon = toAccountIcon;
        _fromAccountCode = fromAccountCode;
        _fromAccountName = fromAccountName;
        _fromAccountIcon = fromAccountIcon;
        _amount = amount;
        _memo = memo;
    }

    public DateTime EntryDate
    {
        get => _entryDate;
        set => SetProperty(ref _entryDate, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string ToAccountCode
    {
        get => _toAccountCode;
        set => SetProperty(ref _toAccountCode, value);
    }

    public string ToAccountName
    {
        get => _toAccountName;
        set
        {
            if (SetProperty(ref _toAccountName, value))
            {
                OnPropertyChanged(nameof(FlowText));
            }
        }
    }

    public string ToAccountIcon
    {
        get => _toAccountIcon;
        set
        {
            if (SetProperty(ref _toAccountIcon, value))
            {
                OnPropertyChanged(nameof(FlowText));
            }
        }
    }

    public string FromAccountCode
    {
        get => _fromAccountCode;
        set => SetProperty(ref _fromAccountCode, value);
    }

    public string FromAccountName
    {
        get => _fromAccountName;
        set
        {
            if (SetProperty(ref _fromAccountName, value))
            {
                OnPropertyChanged(nameof(FlowText));
            }
        }
    }

    public string FromAccountIcon
    {
        get => _fromAccountIcon;
        set
        {
            if (SetProperty(ref _fromAccountIcon, value))
            {
                OnPropertyChanged(nameof(FlowText));
            }
        }
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public string Memo
    {
        get => _memo;
        set => SetProperty(ref _memo, value);
    }

    public string FlowText => $"{FromAccountIcon} {FromAccountName} -> {ToAccountIcon} {ToAccountName}";

    public JournalEntryKind TransactionKind
    {
        get => _transactionKind;
        set => SetProperty(ref _transactionKind, value);
    }
}

public sealed class AutoTransferRule : ObservableObject
{
    private readonly string _id;
    private string _description;
    private string _toAccountCode;
    private string _toAccountName;
    private string _toAccountIcon;
    private string _fromAccountCode;
    private string _fromAccountName;
    private string _fromAccountIcon;
    private decimal _amount;
    private string _memo;
    private int _transferDay;

    public AutoTransferRule(
        string id,
        string description,
        string toAccountCode,
        string toAccountName,
        string toAccountIcon,
        string fromAccountCode,
        string fromAccountName,
        string fromAccountIcon,
        decimal amount,
        string memo,
        int transferDay)
    {
        _id = id;
        _description = description;
        _toAccountCode = toAccountCode;
        _toAccountName = toAccountName;
        _toAccountIcon = toAccountIcon;
        _fromAccountCode = fromAccountCode;
        _fromAccountName = fromAccountName;
        _fromAccountIcon = fromAccountIcon;
        _amount = amount;
        _memo = memo;
        _transferDay = transferDay;
    }

    public string Id => _id;

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string ToAccountCode
    {
        get => _toAccountCode;
        set => SetProperty(ref _toAccountCode, value);
    }

    public string ToAccountName
    {
        get => _toAccountName;
        set
        {
            if (SetProperty(ref _toAccountName, value))
            {
                OnPropertyChanged(nameof(FlowText));
            }
        }
    }

    public string ToAccountIcon
    {
        get => _toAccountIcon;
        set
        {
            if (SetProperty(ref _toAccountIcon, value))
            {
                OnPropertyChanged(nameof(FlowText));
            }
        }
    }

    public string FromAccountCode
    {
        get => _fromAccountCode;
        set => SetProperty(ref _fromAccountCode, value);
    }

    public string FromAccountName
    {
        get => _fromAccountName;
        set
        {
            if (SetProperty(ref _fromAccountName, value))
            {
                OnPropertyChanged(nameof(FlowText));
            }
        }
    }

    public string FromAccountIcon
    {
        get => _fromAccountIcon;
        set
        {
            if (SetProperty(ref _fromAccountIcon, value))
            {
                OnPropertyChanged(nameof(FlowText));
            }
        }
    }

    public decimal Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, value))
            {
                OnPropertyChanged(nameof(AmountText));
            }
        }
    }

    public string Memo
    {
        get => _memo;
        set => SetProperty(ref _memo, value);
    }

    public int TransferDay
    {
        get => _transferDay;
        set
        {
            if (SetProperty(ref _transferDay, value))
            {
                OnPropertyChanged(nameof(TransferDayText));
                OnPropertyChanged(nameof(NextScheduledDate));
                OnPropertyChanged(nameof(NextScheduledDateText));
            }
        }
    }

    public string FlowText => $"{FromAccountIcon} {FromAccountName} -> {ToAccountIcon} {ToAccountName}";

    public string AmountText => $"{Amount:N0}원";

    public string TransferDayText => $"매월 {TransferDay}일";

    public DateTime NextScheduledDate => CalculateNextScheduledDate(DateTime.Today);

    public string NextScheduledDateText => $"{NextScheduledDate:yyyy-MM-dd}";

    public DateTime CalculateNextScheduledDate(DateTime referenceDate)
    {
        var normalized = referenceDate.Date;
        var thisMonthDay = Math.Min(TransferDay, DateTime.DaysInMonth(normalized.Year, normalized.Month));
        var scheduledThisMonth = new DateTime(normalized.Year, normalized.Month, thisMonthDay);
        if (scheduledThisMonth >= normalized)
        {
            return scheduledThisMonth;
        }

        var nextMonth = normalized.AddMonths(1);
        var nextMonthDay = Math.Min(TransferDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
        return new DateTime(nextMonth.Year, nextMonth.Month, nextMonthDay);
    }
}

public sealed class AccountSummary
{
    public AccountSummary(
        string icon,
        string name,
        AccountType accountType,
        string typeLabel,
        decimal incomingTotal,
        decimal outgoingTotal,
        decimal signedBalance)
    {
        Icon = icon;
        Name = name;
        AccountType = accountType;
        TypeLabel = typeLabel;
        IncomingTotal = incomingTotal;
        OutgoingTotal = outgoingTotal;
        SignedBalance = signedBalance;
    }

    public string Icon { get; }

    public string Name { get; }

    public AccountType AccountType { get; }

    public string TypeLabel { get; }

    public decimal IncomingTotal { get; }

    public decimal OutgoingTotal { get; }

    public decimal SignedBalance { get; }

    public decimal Balance => Math.Abs(SignedBalance);

    public bool IsPositiveBalance => SignedBalance >= 0;

    public string BalanceText => $"{(IsPositiveBalance ? "+" : "-")}{Balance:N0}원";
}

public sealed class LedgerMetric
{
    public LedgerMetric(string title, string value, string hint)
    {
        Title = title;
        Value = value;
        Hint = hint;
    }

    public string Title { get; }

    public string Value { get; }

    public string Hint { get; }
}

public sealed class AccountStatusPeriodOption
{
    public AccountStatusPeriodOption(AccountStatusPeriodPreset preset, string label, string description)
    {
        Preset = preset;
        Label = label;
        Description = description;
    }

    public AccountStatusPeriodPreset Preset { get; }

    public string Label { get; }

    public string Description { get; }
}

public sealed class ThemeOption
{
    public ThemeOption(AppThemeMode mode, string label, string description)
    {
        Mode = mode;
        Label = label;
        Description = description;
    }

    public AppThemeMode Mode { get; }

    public string Label { get; }

    public string Description { get; }
}

public sealed class ChartLabel
{
    public ChartLabel(double left, string text)
    {
        Left = left;
        Text = text;
    }

    public double Left { get; }

    public string Text { get; }
}

public sealed class AnalysisChartPoint
{
    private const double HitTargetRadius = 14d;

    public AnalysisChartPoint(double x, double y, DateTime date, decimal amount)
    {
        X = x;
        Y = y;
        Date = date;
        Amount = amount;
    }

    public double X { get; }

    public double Y { get; }

    public DateTime Date { get; }

    public decimal Amount { get; }

    public double CanvasLeft => X - HitTargetRadius;

    public double CanvasTop => Y - HitTargetRadius;

    public string AmountText => $"{Amount:N0}원";

    public string DateText => Date.ToString("yyyy.MM.dd");
}

public sealed class NetAssetComparisonBar
{
    private const double MaxBarWidth = 320d;

    public NetAssetComparisonBar(string label, string caption, decimal amount, double normalizedWidth)
    {
        Label = label;
        Caption = caption;
        Amount = amount;
        NormalizedWidth = normalizedWidth;
    }

    public string Label { get; }

    public string Caption { get; }

    public decimal Amount { get; }

    public double NormalizedWidth { get; }

    public bool IsPositiveBalance => Amount >= 0;

    public string AmountText => $"{(IsPositiveBalance ? "+" : "-")}{Math.Abs(Amount):N0}원";

    public double BarWidth => NormalizedWidth <= 0 ? 0d : Math.Max(28d, MaxBarWidth * NormalizedWidth);
}

public sealed class ExpenseBreakdownItem
{
    public ExpenseBreakdownItem(string name, decimal amount, double sharePercentage, int transactionCount)
    {
        Name = name;
        Amount = amount;
        SharePercentage = sharePercentage;
        TransactionCount = transactionCount;
    }

    public string Name { get; }

    public decimal Amount { get; }

    public double SharePercentage { get; }

    public int TransactionCount { get; }

    public string AmountText => $"{Amount:N0}원";

    public string ShareText => $"{SharePercentage:0.#}%";

    public string TransactionCountText => $"{TransactionCount}건";
}

public sealed class ExpenseCategoryBar
{
    private const double MaxBarWidth = 340d;

    public ExpenseCategoryBar(string name, decimal amount, double sharePercentage, int transactionCount, double normalizedWidth)
    {
        Name = name;
        Amount = amount;
        SharePercentage = sharePercentage;
        TransactionCount = transactionCount;
        NormalizedWidth = normalizedWidth;
    }

    public string Name { get; }

    public decimal Amount { get; }

    public double SharePercentage { get; }

    public int TransactionCount { get; }

    public double NormalizedWidth { get; }

    public string AmountText => $"{Amount:N0}원";

    public string ShareText => $"{SharePercentage:0.#}%";

    public string TransactionCountText => $"{TransactionCount}건";

    public double BarWidth => NormalizedWidth <= 0 ? 0d : Math.Max(42d, MaxBarWidth * NormalizedWidth);
}

public sealed class ExpenseDonutSlice
{
    public ExpenseDonutSlice(string name, decimal amount, double sharePercentage, int transactionCount, Geometry geometry, Brush fill)
    {
        Name = name;
        Amount = amount;
        SharePercentage = sharePercentage;
        TransactionCount = transactionCount;
        Geometry = geometry;
        Fill = fill;
    }

    public string Name { get; }

    public decimal Amount { get; }

    public double SharePercentage { get; }

    public int TransactionCount { get; }

    public Geometry Geometry { get; }

    public Brush Fill { get; }

    public string AmountText => $"{Amount:N0}원";

    public string ShareText => $"{SharePercentage:0.#}%";

    public string TransactionCountText => $"{TransactionCount}건";
}

public sealed class AccountTypeBalanceSummary
{
    public AccountTypeBalanceSummary(string typeLabel, int accountCount, decimal totalBalance, string description)
    {
        TypeLabel = typeLabel;
        AccountCount = accountCount;
        TotalBalance = totalBalance;
        Description = description;
    }

    public string TypeLabel { get; }

    public int AccountCount { get; }

    public decimal TotalBalance { get; }

    public string Description { get; }

    public string AccountCountText => $"{AccountCount}개 계정";

    public string TotalText => $"{(TotalBalance >= 0 ? "+" : "-")}{Math.Abs(TotalBalance):N0}원";

    public bool IsPositiveBalance => TotalBalance >= 0;
}

public static class AccountTypeExtensions
{
    public static string ToKoreanLabel(this AccountType type) => type switch
    {
        AccountType.Asset => "자산",
        AccountType.Investment => "투자",
        AccountType.Liability => "부채",
        AccountType.Equity => "순자산",
        AccountType.Revenue => "수익",
        AccountType.Expense => "지출",
        _ => "기타"
    };
}
