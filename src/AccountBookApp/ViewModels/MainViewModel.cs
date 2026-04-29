
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using AccountBookApp.Infrastructure;
using AccountBookApp.Models;
using Microsoft.Win32;

namespace AccountBookApp.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private const double ChartWidth = 760d;
    private const double ChartHeight = 220d;
    private const double ChartLeftPadding = 12d;
    private const double ChartRightPadding = 12d;
    private const double ChartTopPadding = 18d;
    private const double ChartBottomPadding = 18d;
    private static readonly JsonSerializerOptions DataBackupJsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private DateTime _entryDate = DateTime.Today;
    private string _description = string.Empty;
    private string _amountText = string.Empty;
    private string _memo = string.Empty;
    private string _validationMessage = string.Empty;
    private DateTime _editEntryDate = DateTime.Today;
    private string _editDescription = string.Empty;
    private string _editAmountText = string.Empty;
    private string _editMemo = string.Empty;
    private string _editValidationMessage = string.Empty;
    private int _selectedAutoTransferDay = 25;
    private string _autoTransferDescription = string.Empty;
    private string _autoTransferAmountText = string.Empty;
    private string _autoTransferMemo = string.Empty;
    private string _autoTransferValidationMessage = string.Empty;
    private string _entryHistoryRangeSummary = string.Empty;
    private AccountDefinition? _selectedToAccount;
    private AccountDefinition? _selectedFromAccount;
    private AccountDefinition? _editSelectedToAccount;
    private AccountDefinition? _editSelectedFromAccount;
    private AccountDefinition? _selectedAutoTransferToAccount;
    private AccountDefinition? _selectedAutoTransferFromAccount;
    private JournalEntry? _editingJournalEntry;
    private AutoTransferRule? _editingAutoTransferRule;
    private string _accountValidationMessage = string.Empty;
    private DateTime _analysisStartDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _analysisEndDate = DateTime.Today;
    private DateTime _selectedEntryHistoryMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _selectedAnalysisMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private string _analysisMessage = string.Empty;
    private string _analysisNetAssetSummary = "0원";
    private string _analysisExpenseSummary = "0원";
    private string _analysisRangeSummary = string.Empty;
    private string _accountStatusRangeSummary = string.Empty;
    private string _netAssetTrendHeadline = "순자산 유지";
    private string _netAssetTrendHint = string.Empty;
    private string _expenseDonutCenterTitle = "기록 없음";
    private string _expenseDonutCenterValue = "0원";
    private string _expenseDonutCenterSubtitle = "지출 데이터가 없습니다.";
    private bool _isNetAssetTrendPositive = true;
    private PointCollection _netAssetLinePoints = [];
    private PointCollection _expenseLinePoints = [];
    private AnalysisTab _selectedAnalysisTab = AnalysisTab.NetAsset;
    private AppThemeMode _selectedThemeMode = AppThemeMode.Dark;
    private AppSection _selectedSection = AppSection.Analysis;
    private bool _isAccountPopupOpen;
    private bool _isUserProfilePopupOpen;
    private bool _isAccountEditMode;
    private AccountDefinition? _editingAccount;
    private AccountDefinition? _currentManagedAccount;
    private string _popupAccountNameInput = string.Empty;
    private string _popupAccountDescriptionInput = string.Empty;
    private string _userProfileName = "사용자";
    private string _ledgerName = "개인 장부";
    private string _userProfileNote = "내 장부 정보를 관리합니다.";
    private string _userProfileNameInput = string.Empty;
    private string _ledgerNameInput = string.Empty;
    private string _userProfileNoteInput = string.Empty;
    private string _userProfileValidationMessage = string.Empty;
    private AccountType _popupAccountType = AccountType.Asset;
    private LedgerMetric _netAssetMetric = new("순자산", "0원", "자산 - 부채 기준");
    private LedgerMetric _investmentMetric = new("투자 잔액", "0원", "증권/투자 계정 기준");
    private LedgerMetric _liabilityMetric = new("부채 잔액", "0원", "카드/대출 등 상환 필요 금액");
    private LedgerMetric _autoTransferMetric = new("자동이체", "0건", "총 예정 금액 0원");
    private AccountStatusPeriodOption? _selectedAccountStatusPeriodOption;
    private ThemeOption? _selectedThemeOption;
    private bool _isSyncingEntryHistoryMonth;
    private bool _isSyncingAnalysisMonth;
    private bool _isEntryEditTabSelected;

    public MainViewModel()
    {
        AddEntryCommand = new RelayCommand(AddEntry);
        ImportDataCommand = new RelayCommand(ImportData);
        ExportDataCommand = new RelayCommand(ExportData);
        RefreshAnalysisCommand = new RelayCommand(RefreshAnalysis);
        ApplyThemeCommand = new RelayCommand(ApplyTheme);
        ShowEntrySectionCommand = new RelayCommand(() => SelectedSection = AppSection.Entry);
        ShowAnalysisSectionCommand = new RelayCommand(() => SelectedSection = AppSection.Analysis);
        ShowSettingsSectionCommand = new RelayCommand(() => SelectedSection = AppSection.Settings);
        ShowNetAssetAnalysisTabCommand = new RelayCommand(() => SetAnalysisTab(AnalysisTab.NetAsset));
        ShowExpenseAnalysisTabCommand = new RelayCommand(() => SetAnalysisTab(AnalysisTab.Expense));
        ShowAccountStatusAnalysisTabCommand = new RelayCommand(() => SetAnalysisTab(AnalysisTab.AccountStatus));
        NewAssetAccountCommand = new RelayCommand(() => OpenAccountPopup(AccountType.Asset));
        NewInvestmentAccountCommand = new RelayCommand(() => OpenAccountPopup(AccountType.Investment));
        NewLiabilityAccountCommand = new RelayCommand(() => OpenAccountPopup(AccountType.Liability));
        NewEquityAccountCommand = new RelayCommand(() => OpenAccountPopup(AccountType.Equity));
        NewRevenueAccountCommand = new RelayCommand(() => OpenAccountPopup(AccountType.Revenue));
        NewExpenseAccountCommand = new RelayCommand(() => OpenAccountPopup(AccountType.Expense));
        OpenEditAccountCommand = new RelayCommand(OpenEditCurrentAccount, () => CurrentManagedAccount is not null);
        DeleteAccountCommand = new RelayCommand(DeleteCurrentAccount, () => CurrentManagedAccount is not null);
        SavePopupAccountCommand = new RelayCommand(SavePopupAccount);
        CloseAccountPopupCommand = new RelayCommand(CloseAccountPopup);
        OpenUserProfileCommand = new RelayCommand(OpenUserProfilePopup);
        SaveUserProfileCommand = new RelayCommand(SaveUserProfile);
        CloseUserProfileCommand = new RelayCommand(CloseUserProfilePopup);
        ResetDemoDataCommand = new RelayCommand(ResetDemoData);
        CancelEntryEditCommand = new RelayCommand(CancelJournalEntryEdit, () => IsJournalEntryEditMode);
        SaveEditedEntryCommand = new RelayCommand(SaveEditedEntry, () => IsJournalEntryEditMode);
        ShowEntryInputTabCommand = new RelayCommand(() => IsEntryEditTabSelected = false);
        ShowEntryEditTabCommand = new RelayCommand(() => IsEntryEditTabSelected = true, () => IsJournalEntryEditMode);
        SaveAutoTransferCommand = new RelayCommand(SaveAutoTransfer);
        CancelAutoTransferEditCommand = new RelayCommand(CancelAutoTransferEdit, () => IsAutoTransferEditMode);

        ThemeModes.Add(AppThemeMode.Dark);
        ThemeModes.Add(AppThemeMode.Light);
        ThemeModes.Add(AppThemeMode.Rose);
        SeedAccountStatusPeriodOptions();
        SeedThemeOptions();
        SeedAutoTransferDayOptions();

        SeedAccounts();
        SeedSampleEntries();
        SeedSampleAutoTransfers();
        RefreshAnalysisMonths();
        RefreshEntryHistoryMonths();
        ApplySelectedAnalysisMonthRange(false);
        ResetEntryForm();
        ResetEditEntryForm();
        ResetAutoTransferForm();
        SelectedThemeOption = ThemeOptions.FirstOrDefault(option => option.Mode == SelectedThemeMode);
        ThemeService.ApplyTheme(SelectedThemeMode);
        RefreshDerivedState();
    }

    public ObservableCollection<AccountDefinition> Accounts { get; } = new();

    public ObservableCollection<JournalEntry> JournalEntries { get; } = new();

    public ObservableCollection<AutoTransferRule> AutoTransferRules { get; } = new();

    public ObservableCollection<AccountSummary> AccountSummaries { get; } = new();

    public ObservableCollection<LedgerMetric> Metrics { get; } = new();

    public ObservableCollection<ChartLabel> AnalysisLabels { get; } = new();

    public ObservableCollection<AppThemeMode> ThemeModes { get; } = new();

    public ObservableCollection<ThemeOption> ThemeOptions { get; } = new();

    public ObservableCollection<DateTime> AnalysisMonths { get; } = new();

    public ObservableCollection<DateTime> EntryHistoryMonths { get; } = new();

    public ObservableCollection<int> AutoTransferDayOptions { get; } = new();

    public ObservableCollection<AccountStatusPeriodOption> AccountStatusPeriodOptions { get; } = new();

    public ObservableCollection<LedgerMetric> NetAssetHighlights { get; } = new();

    public ObservableCollection<LedgerMetric> ExpenseHighlights { get; } = new();

    public ObservableCollection<LedgerMetric> AccountStatusHighlights { get; } = new();

    public ObservableCollection<NetAssetComparisonBar> NetAssetComparisonBars { get; } = new();

    public ObservableCollection<AccountSummary> NetAssetFocusAccounts { get; } = new();

    public ObservableCollection<ExpenseBreakdownItem> ExpenseBreakdowns { get; } = new();

    public ObservableCollection<ExpenseCategoryBar> ExpenseCategoryBars { get; } = new();

    public ObservableCollection<ExpenseDonutSlice> ExpenseDonutSlices { get; } = new();

    public ObservableCollection<JournalEntry> RecentExpenseEntries { get; } = new();

    public ObservableCollection<AccountTypeBalanceSummary> AccountTypeSummaries { get; } = new();

    public ObservableCollection<AccountSummary> StatusAccountHighlights { get; } = new();

    public ObservableCollection<JournalEntry> RecentCapitalEntries { get; } = new();

    public ObservableCollection<JournalEntry> FilteredJournalEntries { get; } = new();

    public ObservableCollection<AnalysisChartPoint> NetAssetChartPoints { get; } = new();

    public ObservableCollection<AnalysisChartPoint> ExpenseChartPoints { get; } = new();

    public RelayCommand AddEntryCommand { get; }

    public RelayCommand ImportDataCommand { get; }

    public RelayCommand ExportDataCommand { get; }

    public RelayCommand RefreshAnalysisCommand { get; }

    public RelayCommand ApplyThemeCommand { get; }

    public RelayCommand ShowEntrySectionCommand { get; }

    public RelayCommand ShowAnalysisSectionCommand { get; }

    public RelayCommand ShowSettingsSectionCommand { get; }

    public RelayCommand ShowNetAssetAnalysisTabCommand { get; }

    public RelayCommand ShowExpenseAnalysisTabCommand { get; }

    public RelayCommand ShowAccountStatusAnalysisTabCommand { get; }

    public RelayCommand NewAssetAccountCommand { get; }

    public RelayCommand NewInvestmentAccountCommand { get; }

    public RelayCommand NewLiabilityAccountCommand { get; }

    public RelayCommand NewEquityAccountCommand { get; }

    public RelayCommand NewRevenueAccountCommand { get; }

    public RelayCommand NewExpenseAccountCommand { get; }

    public RelayCommand OpenEditAccountCommand { get; }

    public RelayCommand DeleteAccountCommand { get; }

    public RelayCommand SavePopupAccountCommand { get; }

    public RelayCommand CloseAccountPopupCommand { get; }

    public RelayCommand OpenUserProfileCommand { get; }

    public RelayCommand SaveUserProfileCommand { get; }

    public RelayCommand CloseUserProfileCommand { get; }

    public RelayCommand ResetDemoDataCommand { get; }

    public RelayCommand CancelEntryEditCommand { get; }

    public RelayCommand SaveEditedEntryCommand { get; }

    public RelayCommand ShowEntryInputTabCommand { get; }

    public RelayCommand ShowEntryEditTabCommand { get; }

    public RelayCommand SaveAutoTransferCommand { get; }

    public RelayCommand CancelAutoTransferEditCommand { get; }

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

    public string AmountText
    {
        get => _amountText;
        set => SetProperty(ref _amountText, value);
    }

    public string Memo
    {
        get => _memo;
        set => SetProperty(ref _memo, value);
    }

    public DateTime EditEntryDate
    {
        get => _editEntryDate;
        set => SetProperty(ref _editEntryDate, value);
    }

    public string EditDescription
    {
        get => _editDescription;
        set => SetProperty(ref _editDescription, value);
    }

    public string EditAmountText
    {
        get => _editAmountText;
        set => SetProperty(ref _editAmountText, value);
    }

    public string EditMemo
    {
        get => _editMemo;
        set => SetProperty(ref _editMemo, value);
    }

    public int SelectedAutoTransferDay
    {
        get => _selectedAutoTransferDay;
        set => SetProperty(ref _selectedAutoTransferDay, value);
    }

    public string AutoTransferDescription
    {
        get => _autoTransferDescription;
        set => SetProperty(ref _autoTransferDescription, value);
    }

    public string AutoTransferAmountText
    {
        get => _autoTransferAmountText;
        set => SetProperty(ref _autoTransferAmountText, value);
    }

    public string AutoTransferMemo
    {
        get => _autoTransferMemo;
        set => SetProperty(ref _autoTransferMemo, value);
    }

    public string EntryHistoryRangeSummary
    {
        get => _entryHistoryRangeSummary;
        private set => SetProperty(ref _entryHistoryRangeSummary, value);
    }

    public AccountDefinition? SelectedToAccount
    {
        get => _selectedToAccount;
        set => SetProperty(ref _selectedToAccount, value);
    }

    public AccountDefinition? SelectedFromAccount
    {
        get => _selectedFromAccount;
        set => SetProperty(ref _selectedFromAccount, value);
    }

    public AccountDefinition? EditSelectedToAccount
    {
        get => _editSelectedToAccount;
        set => SetProperty(ref _editSelectedToAccount, value);
    }

    public AccountDefinition? EditSelectedFromAccount
    {
        get => _editSelectedFromAccount;
        set => SetProperty(ref _editSelectedFromAccount, value);
    }

    public AccountDefinition? SelectedAutoTransferToAccount
    {
        get => _selectedAutoTransferToAccount;
        set => SetProperty(ref _selectedAutoTransferToAccount, value);
    }

    public AccountDefinition? SelectedAutoTransferFromAccount
    {
        get => _selectedAutoTransferFromAccount;
        set => SetProperty(ref _selectedAutoTransferFromAccount, value);
    }

    public JournalEntry? EditingJournalEntry
    {
        get => _editingJournalEntry;
        private set
        {
            if (SetProperty(ref _editingJournalEntry, value))
            {
                CancelEntryEditCommand.RaiseCanExecuteChanged();
                SaveEditedEntryCommand.RaiseCanExecuteChanged();
                ShowEntryEditTabCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsJournalEntryEditMode));
                OnPropertyChanged(nameof(EntrySectionTitleText));
                OnPropertyChanged(nameof(EntrySectionDescriptionText));
                OnPropertyChanged(nameof(EntrySubmitButtonText));
                OnPropertyChanged(nameof(EntryEditSummaryText));
                OnPropertyChanged(nameof(SectionSummaryText));
            }
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set
        {
            if (SetProperty(ref _validationMessage, value))
            {
                OnPropertyChanged(nameof(HasValidationMessage));
            }
        }
    }

    public AutoTransferRule? EditingAutoTransferRule
    {
        get => _editingAutoTransferRule;
        private set
        {
            if (SetProperty(ref _editingAutoTransferRule, value))
            {
                CancelAutoTransferEditCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsAutoTransferEditMode));
                OnPropertyChanged(nameof(AutoTransferSubmitButtonText));
                OnPropertyChanged(nameof(AutoTransferEditSummaryText));
            }
        }
    }

    public string EditValidationMessage
    {
        get => _editValidationMessage;
        set
        {
            if (SetProperty(ref _editValidationMessage, value))
            {
                OnPropertyChanged(nameof(HasEditValidationMessage));
            }
        }
    }

    public string AutoTransferValidationMessage
    {
        get => _autoTransferValidationMessage;
        set
        {
            if (SetProperty(ref _autoTransferValidationMessage, value))
            {
                OnPropertyChanged(nameof(HasAutoTransferValidationMessage));
            }
        }
    }

    public string AccountValidationMessage
    {
        get => _accountValidationMessage;
        set
        {
            if (SetProperty(ref _accountValidationMessage, value))
            {
                OnPropertyChanged(nameof(HasAccountValidationMessage));
            }
        }
    }

    public DateTime AnalysisStartDate
    {
        get => _analysisStartDate;
        set => SetProperty(ref _analysisStartDate, value);
    }

    public DateTime SelectedEntryHistoryMonth
    {
        get => _selectedEntryHistoryMonth;
        set
        {
            var normalizedMonth = new DateTime(value.Year, value.Month, 1);
            if (SetProperty(ref _selectedEntryHistoryMonth, normalizedMonth))
            {
                if (!_isSyncingEntryHistoryMonth)
                {
                    RefreshFilteredJournalEntries();
                }
            }
        }
    }

    public DateTime AnalysisEndDate
    {
        get => _analysisEndDate;
        set => SetProperty(ref _analysisEndDate, value);
    }

    public DateTime SelectedAnalysisMonth
    {
        get => _selectedAnalysisMonth;
        set
        {
            var normalizedMonth = new DateTime(value.Year, value.Month, 1);
            if (SetProperty(ref _selectedAnalysisMonth, normalizedMonth))
            {
                OnPropertyChanged(nameof(AnalysisMonthHeadline));

                if (!_isSyncingAnalysisMonth)
                {
                    ApplySelectedAnalysisMonthRange();
                }
            }
        }
    }

    public string AnalysisMessage
    {
        get => _analysisMessage;
        set
        {
            if (SetProperty(ref _analysisMessage, value))
            {
                OnPropertyChanged(nameof(HasAnalysisMessage));
            }
        }
    }

    public string AnalysisNetAssetSummary
    {
        get => _analysisNetAssetSummary;
        private set => SetProperty(ref _analysisNetAssetSummary, value);
    }

    public string AnalysisExpenseSummary
    {
        get => _analysisExpenseSummary;
        private set => SetProperty(ref _analysisExpenseSummary, value);
    }
    public string AnalysisRangeSummary
    {
        get => _analysisRangeSummary;
        private set => SetProperty(ref _analysisRangeSummary, value);
    }

    public string AccountStatusRangeSummary
    {
        get => _accountStatusRangeSummary;
        private set => SetProperty(ref _accountStatusRangeSummary, value);
    }

    public string NetAssetTrendHeadline
    {
        get => _netAssetTrendHeadline;
        private set => SetProperty(ref _netAssetTrendHeadline, value);
    }

    public string NetAssetTrendHint
    {
        get => _netAssetTrendHint;
        private set => SetProperty(ref _netAssetTrendHint, value);
    }

    public string ExpenseDonutCenterTitle
    {
        get => _expenseDonutCenterTitle;
        private set => SetProperty(ref _expenseDonutCenterTitle, value);
    }

    public string ExpenseDonutCenterValue
    {
        get => _expenseDonutCenterValue;
        private set => SetProperty(ref _expenseDonutCenterValue, value);
    }

    public string ExpenseDonutCenterSubtitle
    {
        get => _expenseDonutCenterSubtitle;
        private set => SetProperty(ref _expenseDonutCenterSubtitle, value);
    }

    public bool IsNetAssetTrendPositive
    {
        get => _isNetAssetTrendPositive;
        private set => SetProperty(ref _isNetAssetTrendPositive, value);
    }

    public PointCollection NetAssetLinePoints
    {
        get => _netAssetLinePoints;
        private set => SetProperty(ref _netAssetLinePoints, value);
    }

    public PointCollection ExpenseLinePoints
    {
        get => _expenseLinePoints;
        private set => SetProperty(ref _expenseLinePoints, value);
    }

    public AnalysisTab SelectedAnalysisTab
    {
        get => _selectedAnalysisTab;
        set
        {
            if (SetProperty(ref _selectedAnalysisTab, value))
            {
                OnPropertyChanged(nameof(IsNetAssetAnalysisTabSelected));
                OnPropertyChanged(nameof(IsExpenseAnalysisTabSelected));
                OnPropertyChanged(nameof(IsAccountStatusAnalysisTabSelected));
                OnPropertyChanged(nameof(AnalysisTabDescription));
                OnPropertyChanged(nameof(SectionSummaryText));
            }
        }
    }

    public AccountStatusPeriodOption? SelectedAccountStatusPeriodOption
    {
        get => _selectedAccountStatusPeriodOption;
        set
        {
            if (SetProperty(ref _selectedAccountStatusPeriodOption, value))
            {
                OnPropertyChanged(nameof(AccountStatusPeriodDescription));
                OnPropertyChanged(nameof(AnalysisTabDescription));
                OnPropertyChanged(nameof(SectionSummaryText));

                if (Accounts.Count > 0)
                {
                    RefreshAnalysis();
                }
            }
        }
    }

    public AppThemeMode SelectedThemeMode
    {
        get => _selectedThemeMode;
        set
        {
            if (SetProperty(ref _selectedThemeMode, value))
            {
                ThemeService.ApplyTheme(value);
                var matchingOption = ThemeOptions.FirstOrDefault(option => option.Mode == value);
                if (matchingOption is not null && !ReferenceEquals(matchingOption, SelectedThemeOption))
                {
                    _selectedThemeOption = matchingOption;
                    OnPropertyChanged(nameof(SelectedThemeOption));
                }

                OnPropertyChanged(nameof(SelectedThemeDescription));
            }
        }
    }

    public ThemeOption? SelectedThemeOption
    {
        get => _selectedThemeOption;
        set
        {
            if (SetProperty(ref _selectedThemeOption, value))
            {
                OnPropertyChanged(nameof(SelectedThemeDescription));

                if (value is not null && SelectedThemeMode != value.Mode)
                {
                    SelectedThemeMode = value.Mode;
                }
            }
        }
    }

    public AppSection SelectedSection
    {
        get => _selectedSection;
        set
        {
            if (SetProperty(ref _selectedSection, value))
            {
                OnPropertyChanged(nameof(IsEntrySectionSelected));
                OnPropertyChanged(nameof(IsAnalysisSectionSelected));
                OnPropertyChanged(nameof(IsSettingsSectionSelected));
                OnPropertyChanged(nameof(SectionBreadcrumbText));
                OnPropertyChanged(nameof(SectionTitleText));
                OnPropertyChanged(nameof(SectionSummaryText));
            }
        }
    }

    public bool IsAccountPopupOpen
    {
        get => _isAccountPopupOpen;
        set => SetProperty(ref _isAccountPopupOpen, value);
    }

    public bool IsUserProfilePopupOpen
    {
        get => _isUserProfilePopupOpen;
        set => SetProperty(ref _isUserProfilePopupOpen, value);
    }

    public bool IsAccountEditMode
    {
        get => _isAccountEditMode;
        private set
        {
            if (SetProperty(ref _isAccountEditMode, value))
            {
                OnPropertyChanged(nameof(AccountPopupEyebrowText));
                OnPropertyChanged(nameof(AccountPopupTitle));
                OnPropertyChanged(nameof(AccountPopupDescriptionText));
            }
        }
    }

    public AccountDefinition? EditingAccount
    {
        get => _editingAccount;
        private set => SetProperty(ref _editingAccount, value);
    }

    public AccountDefinition? CurrentManagedAccount
    {
        get => _currentManagedAccount;
        private set
        {
            if (SetProperty(ref _currentManagedAccount, value))
            {
                OpenEditAccountCommand.RaiseCanExecuteChanged();
                DeleteAccountCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string PopupAccountNameInput
    {
        get => _popupAccountNameInput;
        set => SetProperty(ref _popupAccountNameInput, value);
    }

    public string PopupAccountDescriptionInput
    {
        get => _popupAccountDescriptionInput;
        set => SetProperty(ref _popupAccountDescriptionInput, value);
    }

    public string UserProfileName
    {
        get => _userProfileName;
        private set
        {
            if (SetProperty(ref _userProfileName, value))
            {
                OnPropertyChanged(nameof(UserProfileAvatarText));
            }
        }
    }

    public string LedgerName
    {
        get => _ledgerName;
        private set => SetProperty(ref _ledgerName, value);
    }

    public string UserProfileNote
    {
        get => _userProfileNote;
        private set => SetProperty(ref _userProfileNote, value);
    }

    public string UserProfileNameInput
    {
        get => _userProfileNameInput;
        set => SetProperty(ref _userProfileNameInput, value);
    }

    public string LedgerNameInput
    {
        get => _ledgerNameInput;
        set => SetProperty(ref _ledgerNameInput, value);
    }

    public string UserProfileNoteInput
    {
        get => _userProfileNoteInput;
        set => SetProperty(ref _userProfileNoteInput, value);
    }

    public string UserProfileValidationMessage
    {
        get => _userProfileValidationMessage;
        set
        {
            if (SetProperty(ref _userProfileValidationMessage, value))
            {
                OnPropertyChanged(nameof(HasUserProfileValidationMessage));
            }
        }
    }

    public AccountType PopupAccountType
    {
        get => _popupAccountType;
        set
        {
            if (SetProperty(ref _popupAccountType, value))
            {
                OnPropertyChanged(nameof(AccountPopupTitle));
                OnPropertyChanged(nameof(AccountPopupTypeLabel));
                OnPropertyChanged(nameof(AccountPopupDescriptionText));
            }
        }
    }

    public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessage);

    public bool HasEditValidationMessage => !string.IsNullOrWhiteSpace(EditValidationMessage);

    public bool HasAutoTransferValidationMessage => !string.IsNullOrWhiteSpace(AutoTransferValidationMessage);

    public bool HasAccountValidationMessage => !string.IsNullOrWhiteSpace(AccountValidationMessage);

    public bool HasUserProfileValidationMessage => !string.IsNullOrWhiteSpace(UserProfileValidationMessage);

    public bool HasAnalysisMessage => !string.IsNullOrWhiteSpace(AnalysisMessage);

    public bool IsNetAssetAnalysisTabSelected => SelectedAnalysisTab == AnalysisTab.NetAsset;

    public bool IsExpenseAnalysisTabSelected => SelectedAnalysisTab == AnalysisTab.Expense;

    public bool IsAccountStatusAnalysisTabSelected => SelectedAnalysisTab == AnalysisTab.AccountStatus;

    public bool IsEntrySectionSelected => SelectedSection == AppSection.Entry;

    public bool IsAnalysisSectionSelected => SelectedSection == AppSection.Analysis;

    public bool IsSettingsSectionSelected => SelectedSection == AppSection.Settings;

    public bool IsAutoTransferEditMode => EditingAutoTransferRule is not null;

    public bool IsEntryEditTabSelected
    {
        get => _isEntryEditTabSelected;
        private set
        {
            if (SetProperty(ref _isEntryEditTabSelected, value))
            {
                OnPropertyChanged(nameof(IsEntryInputTabSelected));
            }
        }
    }

    public bool IsEntryInputTabSelected => !IsEntryEditTabSelected;

    public IEnumerable<AccountDefinition> ActiveAccounts => Accounts
        .Where(account => !account.IsArchived)
        .OrderBy(account => account.Type)
        .ThenBy(account => account.Code)
        .ToList();

    public IEnumerable<AccountDefinition> AssetAccounts => GetAccountsByType(AccountType.Asset);

    public IEnumerable<AccountDefinition> InvestmentAccounts => GetAccountsByType(AccountType.Investment);

    public IEnumerable<AccountDefinition> LiabilityAccounts => GetAccountsByType(AccountType.Liability);

    public IEnumerable<AccountDefinition> EquityAccounts => GetAccountsByType(AccountType.Equity);

    public IEnumerable<AccountDefinition> RevenueAccounts => GetAccountsByType(AccountType.Revenue);

    public IEnumerable<AccountDefinition> ExpenseAccounts => GetAccountsByType(AccountType.Expense);

    public string HeaderMonthText => $"{DateTime.Today:yyyy년 M월} 장부";

    public string HeaderSummaryText => "핵심 지표와 최근 거래를 한 화면에서 확인할 수 있도록 구성했습니다.";

    public string UserProfileAvatarText => BuildAvatarText(UserProfileName);

    public string SectionBreadcrumbText => SelectedSection switch
    {
        AppSection.Analysis => "대시보드 / 분석",
        AppSection.Entry => "대시보드 / 입력",
        AppSection.Settings => "대시보드 / 설정",
        _ => "대시보드"
    };

    public string SectionTitleText => SelectedSection switch
    {
        AppSection.Analysis => "자산 분석",
        AppSection.Entry => "거래 입력",
        AppSection.Settings => "계정 설정",
        _ => "가계부"
    };

    public string SectionSummaryText => SelectedSection switch
    {
        AppSection.Analysis => AnalysisTabDescription,
        AppSection.Entry => EntrySectionDescriptionText,
        AppSection.Settings => "자산, 투자, 부채, 수익, 비용 계정을 분류별로 관리합니다.",
        _ => HeaderSummaryText
    };

    public string AnalysisMonthHeadline => $"{SelectedAnalysisMonth:yyyy년 M월}";

    public string EntrySectionTitleText => "입력";

    public string EntrySectionDescriptionText => "새 거래를 바로 입력하고, 기존 거래는 최근 거래 목록의 수정 버튼으로 별도 창에서 고칠 수 있습니다.";

    public string EntrySubmitButtonText => "거래 추가";

    public string EntryEditSummaryText => EditingJournalEntry is null
        ? string.Empty
        : $"{EditingJournalEntry.EntryDate:yyyy.MM.dd} · {EditingJournalEntry.Description} · {EditingJournalEntry.Amount:N0}원";

    public bool IsJournalEntryEditMode => EditingJournalEntry is not null;

    public string AutoTransferSubmitButtonText => IsAutoTransferEditMode ? "자동이체 수정" : "자동이체 등록";

    public string AutoTransferEditSummaryText => EditingAutoTransferRule is null
        ? "등록된 자동이체를 목록에서 선택해 수정할 수 있습니다."
        : $"{EditingAutoTransferRule.TransferDayText} · {EditingAutoTransferRule.Description} · {EditingAutoTransferRule.Amount:N0}원";

    public string AutoTransferCountText => $"등록 {AutoTransferRules.Count}건 · 총 예정 금액 {AutoTransferRules.Sum(rule => rule.Amount):N0}원";

    public string SelectedThemeDescription => SelectedThemeOption?.Description
        ?? "원하는 테마를 선택하면 화면 전체 색감이 즉시 반영됩니다.";

    public string AccountStatusPeriodDescription => SelectedAccountStatusPeriodOption?.Description
        ?? "최근 구간을 선택해 주요 계정의 유입, 유출, 현재 잔액을 함께 확인합니다.";

    public string AnalysisTabDescription => SelectedAnalysisTab switch
    {
        AnalysisTab.NetAsset => "자산, 투자, 부채가 순자산에 어떤 영향을 주는지 기간 흐름으로 확인합니다.",
        AnalysisTab.Expense => "지출 추세와 큰 지출 계정, 최근 지출 거래를 한곳에서 봅니다.",
        AnalysisTab.AccountStatus => $"{SelectedAccountStatusPeriodOption?.Label ?? "최근 1개월"} 기준으로 주요 계정 흐름과 유형별 잔액을 점검합니다.",
        _ => string.Empty
    };

    public string LedgerCountText => $"총 {JournalEntries.Count}건";

    public string AccountPopupEyebrowText => IsAccountEditMode ? "계정 수정" : "계정 추가";

    public string AccountPopupTitle => IsAccountEditMode ? $"{PopupAccountType.ToKoreanLabel()} 수정" : $"{PopupAccountType.ToKoreanLabel()} 등록";

    public string AccountPopupDescriptionText => IsAccountEditMode
        ? "선택한 계정의 이름과 설명을 수정합니다. 기존 분개 내역에도 계정명이 반영됩니다."
        : "선택한 계정 분류에 새 계정명과 설명을 추가합니다.";

    public string AccountPopupTypeLabel => PopupAccountType.ToKoreanLabel();

    public string UserProfilePopupEyebrowText => "사용자 관리";

    public string UserProfilePopupTitle => "사용자 계정";

    public string UserProfilePopupDescriptionText => "사이드바 하단 카드에 표시되는 사용자명과 장부명을 수정합니다.";

    public LedgerMetric NetAssetMetric
    {
        get => _netAssetMetric;
        private set => SetProperty(ref _netAssetMetric, value);
    }

    public LedgerMetric LiabilityMetric
    {
        get => _liabilityMetric;
        private set => SetProperty(ref _liabilityMetric, value);
    }

    public LedgerMetric InvestmentMetric
    {
        get => _investmentMetric;
        private set => SetProperty(ref _investmentMetric, value);
    }

    public LedgerMetric AutoTransferMetric
    {
        get => _autoTransferMetric;
        private set => SetProperty(ref _autoTransferMetric, value);
    }

    public void SetCurrentManagedAccount(AccountDefinition? account)
    {
        CurrentManagedAccount = account;
    }

    public void OpenEditCurrentAccount()
    {
        if (CurrentManagedAccount is null)
        {
            return;
        }

        if (IsUserProfilePopupOpen)
        {
            CloseUserProfilePopup();
        }

        IsAccountEditMode = true;
        EditingAccount = CurrentManagedAccount;
        PopupAccountType = CurrentManagedAccount.Type;
        PopupAccountNameInput = CurrentManagedAccount.Name;
        PopupAccountDescriptionInput = CurrentManagedAccount.Description;
        AccountValidationMessage = string.Empty;
        IsAccountPopupOpen = true;
    }

    public void DeleteCurrentAccount()
    {
        if (CurrentManagedAccount is null)
        {
            return;
        }
        var result = MessageBox.Show(
            $"'{CurrentManagedAccount.Name}' 계정을 삭제하면 새 입력에서는 숨겨지지만 기존 거래 기록과 분석에는 유지됩니다. 계속할까요?",
            "계정 삭제",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        CurrentManagedAccount.IsArchived = true;

        if (EditingAccount?.Code == CurrentManagedAccount.Code)
        {
            CloseAccountPopup();
        }

        RefreshAccountSelections();
        RefreshDerivedState();
        CurrentManagedAccount = null;
    }

    public void BeginEditJournalEntry(JournalEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        EditingJournalEntry = entry;
        EditEntryDate = entry.EntryDate;
        EditDescription = entry.Description;
        EditAmountText = entry.Amount.ToString("N0", CultureInfo.CurrentCulture);
        EditMemo = entry.Memo;
        EditSelectedFromAccount = ResolveActiveAccount(entry.FromAccountCode, GetDefaultFromAccount());
        EditSelectedToAccount = ResolveActiveAccount(entry.ToAccountCode, GetDefaultToAccount(entry.FromAccountCode));
        EditValidationMessage = string.Empty;
        SelectedSection = AppSection.Entry;
    }

    public void DeleteJournalEntry(JournalEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"'{entry.Description}' 거래를 삭제할까요? 이 작업은 되돌릴 수 없습니다.",
            "거래 삭제",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        JournalEntries.Remove(entry);

        if (ReferenceEquals(EditingJournalEntry, entry))
        {
            CancelJournalEntryEdit();
        }
        else
        {
            RefreshDerivedState();
        }
    }

    public void BeginEditAutoTransferRule(AutoTransferRule? rule)
    {
        if (rule is null)
        {
            return;
        }

        EditingAutoTransferRule = rule;
        SelectedAutoTransferDay = rule.TransferDay;
        AutoTransferDescription = rule.Description;
        AutoTransferAmountText = rule.Amount.ToString("N0", CultureInfo.CurrentCulture);
        AutoTransferMemo = rule.Memo;
        SelectedAutoTransferFromAccount = ResolveActiveAccount(rule.FromAccountCode, GetDefaultFromAccount());
        SelectedAutoTransferToAccount = ResolveActiveAccount(rule.ToAccountCode, GetDefaultToAccount(rule.FromAccountCode));
        AutoTransferValidationMessage = string.Empty;
        SelectedSection = AppSection.Entry;
    }

    public void DeleteAutoTransferRule(AutoTransferRule? rule)
    {
        if (rule is null)
        {
            return;
        }

        var result = MessageBox.Show(
            $"'{rule.Description}' 자동이체를 삭제할까요?",
            "자동이체 삭제",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        AutoTransferRules.Remove(rule);

        if (ReferenceEquals(EditingAutoTransferRule, rule))
        {
            CancelAutoTransferEdit();
            return;
        }

        RefreshDerivedState();
    }

    private void AddEntry()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Description))
        {
            ValidationMessage = "거래 이름을 입력해 주세요.";
            return;
        }

        if (SelectedFromAccount is null || SelectedToAccount is null)
        {
            ValidationMessage = "왼쪽 계정과 오른쪽 계정을 모두 선택해 주세요.";
            return; 
        }

        if (SelectedFromAccount.Code == SelectedToAccount.Code)
        {
            ValidationMessage = "왼쪽 계정과 오른쪽 계정은 서로 달라야 합니다.";
            return;
        }

        if (!decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) &&
            !decimal.TryParse(AmountText, NumberStyles.Number, CultureInfo.CurrentCulture, out amount))
        {
            ValidationMessage = "금액은 숫자로 입력해 주세요.";
            return;
        }

        if (amount <= 0)
        {
            ValidationMessage = "금액은 0보다 커야 합니다.";
            return;
        }

        var roundedAmount = decimal.Round(amount, 0);
        var entry = new JournalEntry(
            EntryDate.Date,
            Description.Trim(),
            SelectedToAccount.Code,
            SelectedToAccount.Name,
            SelectedToAccount.Icon,
            SelectedFromAccount.Code,
            SelectedFromAccount.Name,
            SelectedFromAccount.Icon,
            roundedAmount,
            Memo.Trim());

        InsertEntrySorted(entry);

        ResetEntryForm();
        RefreshDerivedState();
    }

    private void SaveEditedEntry()
    {
        EditValidationMessage = string.Empty;

        if (EditingJournalEntry is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EditDescription))
        {
            EditValidationMessage = "거래 이름을 입력해 주세요.";
            return;
        }

        if (EditSelectedFromAccount is null || EditSelectedToAccount is null)
        {
            EditValidationMessage = "왼쪽 계정과 오른쪽 계정을 모두 선택해 주세요.";
            return;
        }

        if (EditSelectedFromAccount.Code == EditSelectedToAccount.Code)
        {
            EditValidationMessage = "왼쪽 계정과 오른쪽 계정은 서로 달라야 합니다.";
            return;
        }

        if (!decimal.TryParse(EditAmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) &&
            !decimal.TryParse(EditAmountText, NumberStyles.Number, CultureInfo.CurrentCulture, out amount))
        {
            EditValidationMessage = "금액은 숫자로 입력해 주세요.";
            return;
        }

        if (amount <= 0)
        {
            EditValidationMessage = "금액은 0보다 커야 합니다.";
            return;
        }

        UpdateJournalEntry(
            EditingJournalEntry,
            EditEntryDate.Date,
            EditDescription.Trim(),
            EditSelectedFromAccount,
            EditSelectedToAccount,
            decimal.Round(amount, 0),
            EditMemo.Trim());

        CancelJournalEntryEdit();
    }

    private void SaveAutoTransfer()
    {
        AutoTransferValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(AutoTransferDescription))
        {
            AutoTransferValidationMessage = "자동이체 이름을 입력해 주세요.";
            return;
        }

        if (SelectedAutoTransferFromAccount is null || SelectedAutoTransferToAccount is null)
        {
            AutoTransferValidationMessage = "출금 계정과 입금 계정을 모두 선택해 주세요.";
            return;
        }

        if (SelectedAutoTransferFromAccount.Code == SelectedAutoTransferToAccount.Code)
        {
            AutoTransferValidationMessage = "출금 계정과 입금 계정은 서로 달라야 합니다.";
            return;
        }

        if (!decimal.TryParse(AutoTransferAmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) &&
            !decimal.TryParse(AutoTransferAmountText, NumberStyles.Number, CultureInfo.CurrentCulture, out amount))
        {
            AutoTransferValidationMessage = "금액은 숫자로 입력해 주세요.";
            return;
        }

        if (amount <= 0)
        {
            AutoTransferValidationMessage = "금액은 0보다 커야 합니다.";
            return;
        }

        var roundedAmount = decimal.Round(amount, 0);
        if (EditingAutoTransferRule is null)
        {
            var rule = new AutoTransferRule(
                Guid.NewGuid().ToString("N"),
                AutoTransferDescription.Trim(),
                SelectedAutoTransferToAccount.Code,
                SelectedAutoTransferToAccount.Name,
                SelectedAutoTransferToAccount.Icon,
                SelectedAutoTransferFromAccount.Code,
                SelectedAutoTransferFromAccount.Name,
                SelectedAutoTransferFromAccount.Icon,
                roundedAmount,
                AutoTransferMemo.Trim(),
                SelectedAutoTransferDay);

            InsertAutoTransferSorted(rule);
        }
        else
        {
            UpdateAutoTransferRule(
                EditingAutoTransferRule,
                AutoTransferDescription.Trim(),
                SelectedAutoTransferFromAccount,
                SelectedAutoTransferToAccount,
                roundedAmount,
                AutoTransferMemo.Trim(),
                SelectedAutoTransferDay);
        }

        ResetAutoTransferForm();
        RefreshDerivedState();
    }

    private void OpenAccountPopup(AccountType type)
    {
        if (IsUserProfilePopupOpen)
        {
            CloseUserProfilePopup();
        }

        IsAccountEditMode = false;
        EditingAccount = null;
        PopupAccountType = type;
        PopupAccountNameInput = string.Empty;
        PopupAccountDescriptionInput = string.Empty;
        AccountValidationMessage = string.Empty;
        IsAccountPopupOpen = true;
    }

    private void CloseAccountPopup()
    {
        IsAccountPopupOpen = false;
        IsAccountEditMode = false;
        EditingAccount = null;
        PopupAccountNameInput = string.Empty;
        PopupAccountDescriptionInput = string.Empty;
        AccountValidationMessage = string.Empty;
    }

    private void OpenUserProfilePopup()
    {
        if (IsAccountPopupOpen)
        {
            CloseAccountPopup();
        }

        UserProfileNameInput = UserProfileName;
        LedgerNameInput = LedgerName;
        UserProfileNoteInput = UserProfileNote;
        UserProfileValidationMessage = string.Empty;
        IsUserProfilePopupOpen = true;
    }

    private void CloseUserProfilePopup()
    {
        IsUserProfilePopupOpen = false;
        UserProfileNameInput = string.Empty;
        LedgerNameInput = string.Empty;
        UserProfileNoteInput = string.Empty;
        UserProfileValidationMessage = string.Empty;
    }

    private void SaveUserProfile()
    {
        UserProfileValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(UserProfileNameInput))
        {
            UserProfileValidationMessage = "사용자 이름을 입력해 주세요.";
            return;
        }

        if (string.IsNullOrWhiteSpace(LedgerNameInput))
        {
            UserProfileValidationMessage = "장부 이름을 입력해 주세요.";
            return;
        }

        UserProfileName = UserProfileNameInput.Trim();
        LedgerName = LedgerNameInput.Trim();
        UserProfileNote = string.IsNullOrWhiteSpace(UserProfileNoteInput)
            ? "내 장부 정보를 관리합니다."
            : UserProfileNoteInput.Trim();

        CloseUserProfilePopup();
    }

    private void SavePopupAccount()
    {
        AccountValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(PopupAccountNameInput))
        {
            AccountValidationMessage = "계정 이름을 입력해 주세요.";
            return;
        }

        var name = PopupAccountNameInput.Trim();
        var duplicateExists = Accounts.Any(account =>
            !account.IsArchived &&
            account.Type == PopupAccountType &&
            string.Equals(account.Name, name, StringComparison.OrdinalIgnoreCase) &&
            account.Code != EditingAccount?.Code);

        if (duplicateExists)
        {
            AccountValidationMessage = "같은 분류에 같은 이름의 계정이 이미 있습니다.";
            return;
        }

        if (IsAccountEditMode && EditingAccount is not null)
        {
            EditingAccount.Name = name;
            EditingAccount.Description = PopupAccountDescriptionInput.Trim();
            UpdateJournalEntriesForAccount(EditingAccount);
            UpdateAutoTransferRulesForAccount(EditingAccount);
        }
        else
        {
            var newAccount = new AccountDefinition(
                GenerateNextCode(PopupAccountType),
                name,
                PopupAccountDescriptionInput.Trim(),
                PopupAccountType,
                GenerateIcon(PopupAccountType));

            Accounts.Add(newAccount);
        }

        RefreshAccountSelections();
        RefreshDerivedState();
        CloseAccountPopup();
    }

    private void ImportData()
    {
        var dialog = new OpenFileDialog
        {
            Title = "AccountBook 데이터 가져오기",
            Filter = "AccountBook 백업 (*.accountbook.json)|*.accountbook.json|JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        AccountBookBackup? backup;
        try
        {
            var json = File.ReadAllText(dialog.FileName, Encoding.UTF8);
            backup = JsonSerializer.Deserialize<AccountBookBackup>(json, DataBackupJsonOptions);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"데이터 파일을 읽을 수 없습니다.\n{ex.Message}",
                "데이터 가져오기",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (!TryValidateBackup(backup, out var validationMessage))
        {
            MessageBox.Show(validationMessage, "데이터 가져오기", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            "현재 작성 중인 계정, 거래, 자동이체, 프로필 정보가 가져온 데이터로 교체됩니다. 계속할까요?",
            "데이터 가져오기",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        ApplyDataBackup(backup!);
        MessageBox.Show("데이터를 가져왔습니다.", "데이터 가져오기", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportData()
    {
        var dialog = new SaveFileDialog
        {
            Title = "AccountBook 데이터 내보내기",
            Filter = "AccountBook 백업 (*.accountbook.json)|*.accountbook.json|JSON 파일 (*.json)|*.json",
            FileName = $"AccountBook-backup-{DateTime.Now:yyyyMMdd-HHmmss}.accountbook.json",
            AddExtension = true,
            DefaultExt = ".accountbook.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var json = JsonSerializer.Serialize(CreateDataBackup(), DataBackupJsonOptions);
        File.WriteAllText(dialog.FileName, json, new UTF8Encoding(true));
        MessageBox.Show("데이터를 내보냈습니다.", "데이터 내보내기", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private AccountBookBackup CreateDataBackup()
    {
        return new AccountBookBackup
        {
            Version = 1,
            ExportedAt = DateTime.Now,
            ThemeMode = SelectedThemeMode,
            UserProfileName = UserProfileName,
            LedgerName = LedgerName,
            UserProfileNote = UserProfileNote,
            Accounts = Accounts
                .Select(account => new AccountBackupItem
                {
                    Code = account.Code,
                    Name = account.Name,
                    Description = account.Description,
                    Type = account.Type,
                    Icon = account.Icon,
                    IsArchived = account.IsArchived
                })
                .ToList(),
            JournalEntries = JournalEntries
                .OrderBy(entry => entry.EntryDate)
                .ThenBy(entry => entry.Description)
                .Select(entry => new JournalEntryBackupItem
                {
                    EntryDate = entry.EntryDate.Date,
                    Description = entry.Description,
                    ToAccountCode = entry.ToAccountCode,
                    ToAccountName = entry.ToAccountName,
                    ToAccountIcon = entry.ToAccountIcon,
                    FromAccountCode = entry.FromAccountCode,
                    FromAccountName = entry.FromAccountName,
                    FromAccountIcon = entry.FromAccountIcon,
                    Amount = entry.Amount,
                    Memo = entry.Memo
                })
                .ToList(),
            AutoTransferRules = AutoTransferRules
                .OrderBy(rule => rule.TransferDay)
                .ThenBy(rule => rule.Description)
                .Select(rule => new AutoTransferRuleBackupItem
                {
                    Id = rule.Id,
                    Description = rule.Description,
                    ToAccountCode = rule.ToAccountCode,
                    ToAccountName = rule.ToAccountName,
                    ToAccountIcon = rule.ToAccountIcon,
                    FromAccountCode = rule.FromAccountCode,
                    FromAccountName = rule.FromAccountName,
                    FromAccountIcon = rule.FromAccountIcon,
                    Amount = rule.Amount,
                    Memo = rule.Memo,
                    TransferDay = rule.TransferDay
                })
                .ToList()
        };
    }

    private static bool TryValidateBackup(AccountBookBackup? backup, out string message)
    {
        if (backup is null)
        {
            message = "AccountBook 데이터 파일이 아닙니다.";
            return false;
        }

        if (backup.Accounts is not { Count: > 0 })
        {
            message = "가져올 계정 정보가 없습니다.";
            return false;
        }

        var invalidAccount = backup.Accounts.FirstOrDefault(account =>
            string.IsNullOrWhiteSpace(account.Code) || string.IsNullOrWhiteSpace(account.Name));
        if (invalidAccount is not null)
        {
            message = "계정 코드와 이름이 비어 있는 항목이 있습니다.";
            return false;
        }

        var duplicateCode = backup.Accounts
            .GroupBy(account => account.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateCode is not null)
        {
            message = $"중복된 계정 코드가 있습니다: {duplicateCode.Key}";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private void ApplyDataBackup(AccountBookBackup backup)
    {
        CloseAccountPopup();
        CloseUserProfilePopup();
        EditingJournalEntry = null;
        EditingAutoTransferRule = null;
        IsEntryEditTabSelected = false;

        Accounts.Clear();
        foreach (var item in backup.Accounts)
        {
            var account = new AccountDefinition(
                item.Code.Trim(),
                item.Name.Trim(),
                item.Description?.Trim() ?? string.Empty,
                item.Type,
                string.IsNullOrWhiteSpace(item.Icon) ? GenerateIcon(item.Type) : item.Icon.Trim());

            account.IsArchived = item.IsArchived;
            Accounts.Add(account);
        }

        JournalEntries.Clear();
        foreach (var item in backup.JournalEntries ?? [])
        {
            var fromAccount = Accounts.FirstOrDefault(account => account.Code == item.FromAccountCode);
            var toAccount = Accounts.FirstOrDefault(account => account.Code == item.ToAccountCode);

            InsertEntrySorted(new JournalEntry(
                item.EntryDate.Date,
                item.Description?.Trim() ?? string.Empty,
                item.ToAccountCode?.Trim() ?? string.Empty,
                toAccount?.Name ?? item.ToAccountName ?? string.Empty,
                toAccount?.Icon ?? item.ToAccountIcon ?? string.Empty,
                item.FromAccountCode?.Trim() ?? string.Empty,
                fromAccount?.Name ?? item.FromAccountName ?? string.Empty,
                fromAccount?.Icon ?? item.FromAccountIcon ?? string.Empty,
                Math.Max(0m, decimal.Round(item.Amount, 0)),
                item.Memo?.Trim() ?? string.Empty));
        }

        AutoTransferRules.Clear();
        foreach (var item in backup.AutoTransferRules ?? [])
        {
            var fromAccount = Accounts.FirstOrDefault(account => account.Code == item.FromAccountCode);
            var toAccount = Accounts.FirstOrDefault(account => account.Code == item.ToAccountCode);

            InsertAutoTransferSorted(new AutoTransferRule(
                string.IsNullOrWhiteSpace(item.Id) ? Guid.NewGuid().ToString("N") : item.Id.Trim(),
                item.Description?.Trim() ?? string.Empty,
                item.ToAccountCode?.Trim() ?? string.Empty,
                toAccount?.Name ?? item.ToAccountName ?? string.Empty,
                toAccount?.Icon ?? item.ToAccountIcon ?? string.Empty,
                item.FromAccountCode?.Trim() ?? string.Empty,
                fromAccount?.Name ?? item.FromAccountName ?? string.Empty,
                fromAccount?.Icon ?? item.FromAccountIcon ?? string.Empty,
                Math.Max(0m, decimal.Round(item.Amount, 0)),
                item.Memo?.Trim() ?? string.Empty,
                Math.Clamp(item.TransferDay, 1, 31)));
        }

        UserProfileName = string.IsNullOrWhiteSpace(backup.UserProfileName) ? "사용자" : backup.UserProfileName.Trim();
        LedgerName = string.IsNullOrWhiteSpace(backup.LedgerName) ? "개인 장부" : backup.LedgerName.Trim();
        UserProfileNote = string.IsNullOrWhiteSpace(backup.UserProfileNote)
            ? "내 장부 정보를 관리합니다."
            : backup.UserProfileNote.Trim();

        SelectedThemeMode = backup.ThemeMode;
        SelectedThemeOption = ThemeOptions.FirstOrDefault(option => option.Mode == SelectedThemeMode);
        SelectedAccountStatusPeriodOption ??= AccountStatusPeriodOptions.FirstOrDefault();

        RefreshAnalysisMonths();
        RefreshEntryHistoryMonths();
        ApplySelectedAnalysisMonthRange(false);
        ResetEntryForm();
        ResetEditEntryForm();
        ResetAutoTransferForm();
        RefreshAccountSelections();
        RefreshDerivedState();
        SelectedSection = AppSection.Settings;
    }

    private void RefreshAnalysis()
    {
        AnalysisMessage = string.Empty;

        if (AnalysisStartDate.Date > AnalysisEndDate.Date)
        {
            AnalysisMessage = "시작일은 종료일보다 늦을 수 없습니다.";
            return;
        }

        var dates = EachDay(AnalysisStartDate.Date, AnalysisEndDate.Date).ToList();
        if (dates.Count == 0)
        {
            AnalysisMessage = "분석 기간을 다시 선택해 주세요.";
            return;
        }

        var netAssetValues = new List<decimal>(dates.Count);
        var expenseValues = new List<decimal>(dates.Count);
        var rangeEntries = JournalEntries
            .Where(entry => entry.EntryDate.Date >= AnalysisStartDate.Date && entry.EntryDate.Date <= AnalysisEndDate.Date)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.Amount)
            .ToList();

        foreach (var date in dates)
        {
            netAssetValues.Add(CalculateNetAssetOn(date));
            expenseValues.Add(CalculateExpenseOn(date));
        }

        var accountSnapshot = BuildAccountSummaries(AnalysisEndDate.Date);
        var accountStatusPeriodStartDate = GetAccountStatusPeriodStartDate(AnalysisEndDate.Date);
        var accountStatusSnapshot = BuildAccountSummaries(accountStatusPeriodStartDate, AnalysisEndDate.Date);

        NetAssetLinePoints = BuildLinePoints(netAssetValues);
        ExpenseLinePoints = BuildLinePoints(expenseValues);
        RefreshChartPoints(NetAssetChartPoints, dates, netAssetValues);
        RefreshChartPoints(ExpenseChartPoints, dates, expenseValues);
        RefreshAnalysisLabels(dates);

        AnalysisNetAssetSummary = $"{netAssetValues.LastOrDefault():N0}원";
        AnalysisExpenseSummary = $"{expenseValues.Sum():N0}원";
        AnalysisRangeSummary = $"{AnalysisStartDate:yyyy.MM.dd} - {AnalysisEndDate:yyyy.MM.dd} · {dates.Count}일";
        RefreshNetAssetTabData(netAssetValues, accountSnapshot, rangeEntries);
        RefreshExpenseTabData(dates, rangeEntries);
        RefreshAccountStatusTabData(accountStatusSnapshot, accountStatusPeriodStartDate, AnalysisEndDate.Date);
    }

    private void SetAnalysisTab(AnalysisTab tab)
    {
        SelectedAnalysisTab = tab;

        if (tab == AnalysisTab.NetAsset)
        {
            ApplySelectedAnalysisMonthRange();
        }
    }

    private void SeedAccountStatusPeriodOptions()
    {
        AccountStatusPeriodOptions.Clear();
        AccountStatusPeriodOptions.Add(new AccountStatusPeriodOption(
            AccountStatusPeriodPreset.OneMonth,
            "최근 1개월",
            "최근 1개월 안에 움직임이 있던 계정의 유입, 유출, 현재 잔액을 함께 봅니다."));
        AccountStatusPeriodOptions.Add(new AccountStatusPeriodOption(
            AccountStatusPeriodPreset.ThreeMonths,
            "최근 3개월",
            "최근 3개월 안에서 자주 움직인 계정을 중심으로 흐름을 정리합니다."));
        AccountStatusPeriodOptions.Add(new AccountStatusPeriodOption(
            AccountStatusPeriodPreset.SixMonths,
            "최근 6개월",
            "반년 동안의 계정 변화와 현재 잔액을 한 번에 비교합니다."));
        AccountStatusPeriodOptions.Add(new AccountStatusPeriodOption(
            AccountStatusPeriodPreset.OneYear,
            "최근 1년",
            "1년 기준으로 주요 계정의 유입, 유출, 잔액을 넓게 점검합니다."));
        AccountStatusPeriodOptions.Add(new AccountStatusPeriodOption(
            AccountStatusPeriodPreset.All,
            "전체",
            "전체 기록 기준으로 모든 활성 계정의 누적 흐름과 잔액을 확인합니다."));
        SelectedAccountStatusPeriodOption = AccountStatusPeriodOptions.FirstOrDefault();
    }

    private void SeedThemeOptions()
    {
        ThemeOptions.Clear();
        ThemeOptions.Add(new ThemeOption(
            AppThemeMode.Dark,
            "다크",
            "차분한 남색 기반 테마로 분석 화면과 차트 대비가 또렷하게 보입니다."));
        ThemeOptions.Add(new ThemeOption(
            AppThemeMode.Light,
            "라이트",
            "밝고 깨끗한 톤으로 표와 입력 폼을 편안하게 읽을 수 있습니다."));
        ThemeOptions.Add(new ThemeOption(
            AppThemeMode.Rose,
            "연분홍",
            "부드러운 핑크 톤으로 가계부를 더 가볍고 개인적인 분위기로 볼 수 있습니다."));
    }

    private void RefreshAnalysisMonths()
    {
        AnalysisMonths.Clear();

        var earliestEntryDate = JournalEntries.Count == 0
            ? DateTime.Today
            : JournalEntries.Min(entry => entry.EntryDate);
        var latestEntryDate = JournalEntries.Count == 0
            ? DateTime.Today
            : JournalEntries.Max(entry => entry.EntryDate);
        var firstMonth = new DateTime(earliestEntryDate.Year, earliestEntryDate.Month, 1);
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var lastMonth = latestEntryDate > DateTime.Today
            ? new DateTime(latestEntryDate.Year, latestEntryDate.Month, 1)
            : currentMonth;

        for (var month = lastMonth; month >= firstMonth; month = month.AddMonths(-1))
        {
            AnalysisMonths.Add(month);
        }

        if (!AnalysisMonths.Contains(SelectedAnalysisMonth))
        {
            _isSyncingAnalysisMonth = true;
            SelectedAnalysisMonth = currentMonth;
            _isSyncingAnalysisMonth = false;
        }
    }

    private void RefreshEntryHistoryMonths()
    {
        EntryHistoryMonths.Clear();

        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var earliestMonth = JournalEntries.Count == 0
            ? currentMonth
            : new DateTime(JournalEntries.Min(entry => entry.EntryDate).Year, JournalEntries.Min(entry => entry.EntryDate).Month, 1);
        var latestJournalMonth = JournalEntries.Count == 0
            ? currentMonth
            : new DateTime(JournalEntries.Max(entry => entry.EntryDate).Year, JournalEntries.Max(entry => entry.EntryDate).Month, 1);
        var latestMonth = latestJournalMonth > currentMonth ? latestJournalMonth : currentMonth;

        for (var month = latestMonth; month >= earliestMonth; month = month.AddMonths(-1))
        {
            EntryHistoryMonths.Add(month);
        }

        var targetMonth = EntryHistoryMonths.Contains(currentMonth)
            ? currentMonth
            : EntryHistoryMonths.FirstOrDefault();

        if (!EntryHistoryMonths.Contains(SelectedEntryHistoryMonth))
        {
            _isSyncingEntryHistoryMonth = true;
            SelectedEntryHistoryMonth = targetMonth == default ? currentMonth : targetMonth;
            _isSyncingEntryHistoryMonth = false;
        }

        RefreshFilteredJournalEntries();
    }

    private void RefreshFilteredJournalEntries()
    {
        FilteredJournalEntries.Clear();

        var monthStart = new DateTime(SelectedEntryHistoryMonth.Year, SelectedEntryHistoryMonth.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        foreach (var entry in JournalEntries
                     .Where(entry => entry.EntryDate.Date >= monthStart && entry.EntryDate.Date <= monthEnd)
                     .OrderByDescending(entry => entry.EntryDate)
                     .ThenByDescending(entry => entry.Amount))
        {
            FilteredJournalEntries.Add(entry);
        }

        EntryHistoryRangeSummary = $"{monthStart:yyyy.MM.dd} - {monthEnd:yyyy.MM.dd} · {FilteredJournalEntries.Count}건";
    }

    private void ApplySelectedAnalysisMonthRange(bool refresh = true)
    {
        var monthStart = new DateTime(SelectedAnalysisMonth.Year, SelectedAnalysisMonth.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var today = DateTime.Today;
        var effectiveEnd = monthStart.Year == today.Year && monthStart.Month == today.Month ? today : monthEnd;

        _isSyncingAnalysisMonth = true;
        AnalysisStartDate = monthStart;
        AnalysisEndDate = effectiveEnd;
        _isSyncingAnalysisMonth = false;

        if (refresh)
        {
            RefreshAnalysis();
        }
    }

    private DateTime? GetAccountStatusPeriodStartDate(DateTime endDate)
    {
        return SelectedAccountStatusPeriodOption?.Preset switch
        {
            AccountStatusPeriodPreset.OneMonth => endDate.AddMonths(-1).AddDays(1),
            AccountStatusPeriodPreset.ThreeMonths => endDate.AddMonths(-3).AddDays(1),
            AccountStatusPeriodPreset.SixMonths => endDate.AddMonths(-6).AddDays(1),
            AccountStatusPeriodPreset.OneYear => endDate.AddYears(-1).AddDays(1),
            AccountStatusPeriodPreset.All or null => null,
            _ => null
        };
    }

    private void RefreshNetAssetTabData(
        IReadOnlyList<decimal> netAssetValues,
        IReadOnlyList<AccountSummary> accountSnapshot,
        IReadOnlyList<JournalEntry> rangeEntries)
    {
        var startingNetAsset = CalculateNetAssetOn(AnalysisStartDate.Date.AddDays(-1));
        var endingNetAsset = netAssetValues.LastOrDefault();
        var netAssetChange = endingNetAsset - startingNetAsset;
        var investmentBalance = accountSnapshot
            .Where(item => item.AccountType == AccountType.Investment)
            .Sum(item => item.SignedBalance);
        var liabilityBalance = accountSnapshot
            .Where(item => item.AccountType == AccountType.Liability)
            .Sum(item => item.SignedBalance);
        var liquidAndInvestmentAssets = accountSnapshot
            .Where(item => item.AccountType is AccountType.Asset or AccountType.Investment)
            .Sum(item => item.SignedBalance);
        var investmentShare = liquidAndInvestmentAssets <= 0
            ? 0d
            : (double)(investmentBalance / liquidAndInvestmentAssets * 100m);
        var netAssetBarMax = new[] { Math.Abs(startingNetAsset), Math.Abs(endingNetAsset), Math.Abs(netAssetChange) }
            .DefaultIfEmpty(0m)
            .Max();

        NetAssetHighlights.Clear();
        NetAssetHighlights.Add(new LedgerMetric("종료일 순자산", $"{endingNetAsset:N0}원", $"기간 변동 {FormatSignedCurrency(netAssetChange)}"));
        NetAssetHighlights.Add(new LedgerMetric("투자 잔액", $"{investmentBalance:N0}원", "증권/투자 계정 기준"));
        NetAssetHighlights.Add(new LedgerMetric("부채 총액", $"{liabilityBalance:N0}원", "순자산 계산에서 차감되는 금액"));
        NetAssetHighlights.Add(new LedgerMetric("투자 비중", $"{investmentShare:0.#}%", "자산 + 투자 잔액 대비 비중"));

        IsNetAssetTrendPositive = netAssetChange >= 0;
        NetAssetTrendHeadline = netAssetChange switch
        {
            > 0 => "순자산이 증가하고 있어요",
            < 0 => "순자산이 감소하고 있어요",
            _ => "순자산이 유지되고 있어요"
        };
        NetAssetTrendHint = netAssetChange == 0
            ? "선택한 기간 동안 순자산 변화가 없습니다."
            : $"선택한 기간 동안 {FormatSignedCurrency(netAssetChange)} 변했습니다.";

        NetAssetComparisonBars.Clear();
        NetAssetComparisonBars.Add(new NetAssetComparisonBar(
            "시작 순자산",
            "선택 기간 시작 시점",
            startingNetAsset,
            NormalizeBarWidth(startingNetAsset, netAssetBarMax)));
        NetAssetComparisonBars.Add(new NetAssetComparisonBar(
            "종료 순자산",
            "선택 기간 종료 시점",
            endingNetAsset,
            NormalizeBarWidth(endingNetAsset, netAssetBarMax)));
        NetAssetComparisonBars.Add(new NetAssetComparisonBar(
            "순증감",
            "종료 순자산 - 시작 순자산",
            netAssetChange,
            NormalizeBarWidth(netAssetChange, netAssetBarMax)));

        NetAssetFocusAccounts.Clear();
        foreach (var accountSummary in accountSnapshot
                     .Where(item => item.AccountType is AccountType.Asset or AccountType.Investment or AccountType.Liability)
                     .OrderByDescending(item => Math.Abs(item.SignedBalance))
                     .Take(8))
        {
            NetAssetFocusAccounts.Add(accountSummary);
        }

        RecentCapitalEntries.Clear();
        foreach (var entry in rangeEntries
                     .Where(entry =>
                     {
                         var toType = GetAccountType(entry.ToAccountCode);
                         var fromType = GetAccountType(entry.FromAccountCode);
                         return toType is AccountType.Asset or AccountType.Investment or AccountType.Liability
                             || fromType is AccountType.Asset or AccountType.Investment or AccountType.Liability;
                     })
                     .Take(8))
        {
            RecentCapitalEntries.Add(entry);
        }
    }

    private void RefreshExpenseTabData(IReadOnlyList<DateTime> dates, IReadOnlyList<JournalEntry> rangeEntries)
    {
        var expenseEntries = rangeEntries
            .Where(entry => GetAccountType(entry.ToAccountCode) == AccountType.Expense)
            .ToList();
        var totalExpense = expenseEntries.Sum(entry => entry.Amount);
        var averageExpense = dates.Count == 0 ? 0m : decimal.Round(totalExpense / dates.Count, 0);
        var largestExpense = expenseEntries
            .OrderByDescending(entry => entry.Amount)
            .FirstOrDefault();
        var groupedExpenses = expenseEntries
            .GroupBy(entry => entry.ToAccountName)
            .Select(group => (Name: group.Key, Amount: group.Sum(entry => entry.Amount), Count: group.Count()))
            .OrderByDescending(group => group.Amount)
            .Take(6)
            .ToList();
        var topExpenseGroup = groupedExpenses.FirstOrDefault();
        var topExpenseAmount = groupedExpenses
            .Select(group => group.Amount)
            .DefaultIfEmpty(0m)
            .Max();

        ExpenseHighlights.Clear();
        ExpenseHighlights.Add(new LedgerMetric("기간 지출", $"{totalExpense:N0}원", "선택한 기간 동안 비용 계정으로 이동한 금액"));
        ExpenseHighlights.Add(new LedgerMetric("일평균 지출", $"{averageExpense:N0}원", "분석 기간 전체 일수 기준"));
        ExpenseHighlights.Add(new LedgerMetric(
            "가장 큰 지출 계정",
            string.IsNullOrWhiteSpace(topExpenseGroup.Name) ? "데이터 없음" : topExpenseGroup.Name,
            string.IsNullOrWhiteSpace(topExpenseGroup.Name) ? "아직 기록된 지출이 없습니다." : $"{topExpenseGroup.Amount:N0}원 · {topExpenseGroup.Count}건"));
        ExpenseHighlights.Add(new LedgerMetric(
            "최대 단건 지출",
            largestExpense is null ? "데이터 없음" : $"{largestExpense.Amount:N0}원",
            largestExpense is null ? "아직 기록된 지출이 없습니다." : $"{largestExpense.Description} · {largestExpense.EntryDate:MM.dd}"));

        ExpenseCategoryBars.Clear();
        foreach (var group in groupedExpenses)
        {
            ExpenseCategoryBars.Add(new ExpenseCategoryBar(
                group.Name,
                group.Amount,
                totalExpense <= 0 ? 0d : (double)(group.Amount / totalExpense * 100m),
                group.Count,
                NormalizeBarWidth(group.Amount, topExpenseAmount)));
        }

        ExpenseBreakdowns.Clear();
        foreach (var group in groupedExpenses.Select(group => new ExpenseBreakdownItem(
                     group.Name,
                     group.Amount,
                     totalExpense <= 0 ? 0d : (double)(group.Amount / totalExpense * 100m),
                     group.Count)))
        {
            ExpenseBreakdowns.Add(group);
        }

        RecentExpenseEntries.Clear();
        foreach (var entry in expenseEntries.Take(8))
        {
            RecentExpenseEntries.Add(entry);
        }

        ExpenseDonutCenterTitle = totalExpense <= 0 ? "기록 없음" : "총 지출";
        ExpenseDonutCenterValue = $"{totalExpense:N0}원";
        ExpenseDonutCenterSubtitle = string.IsNullOrWhiteSpace(topExpenseGroup.Name)
            ? "지출 데이터가 없습니다."
            : $"가장 큰 비중 · {topExpenseGroup.Name}";
        RefreshExpenseDonutSlices(groupedExpenses, totalExpense);
    }

    private void RefreshExpenseDonutSlices(IReadOnlyList<(string Name, decimal Amount, int Count)> groupedExpenses, decimal totalExpense)
    {
        ExpenseDonutSlices.Clear();

        if (groupedExpenses.Count == 0 || totalExpense <= 0)
        {
            return;
        }

        var palette = new[]
        {
            "#4DA3FF",
            "#BE5AF2",
            "#5AD7A1",
            "#F6C453",
            "#FF7A7A",
            "#8FD1FF"
        };

        var renderSweepAngles = BuildVisibleSweepAngles(groupedExpenses.Select(group => group.Amount).ToList(), totalExpense);
        var startAngle = -90d;
        for (var index = 0; index < groupedExpenses.Count; index++)
        {
            var group = groupedExpenses[index];
            var sweepAngle = renderSweepAngles[index];

            ExpenseDonutSlices.Add(new ExpenseDonutSlice(
                group.Name,
                group.Amount,
                (double)(group.Amount / totalExpense * 100m),
                group.Count,
                BuildDonutSliceGeometry(startAngle, sweepAngle, 96d, 96d, 82d, 52d),
                CreateChartBrush(palette[index % palette.Length])));

            startAngle += sweepAngle;
        }
    }

    private void RefreshAccountStatusTabData(
        IReadOnlyList<AccountSummary> accountSnapshot,
        DateTime? periodStartDate,
        DateTime periodEndDate)
    {
        var periodLabel = SelectedAccountStatusPeriodOption?.Label ?? "최근 1개월";
        var activeAccountCount = accountSnapshot.Count;
        var investmentAccountCount = accountSnapshot.Count(item => item.AccountType == AccountType.Investment);
        var positiveBalanceCount = accountSnapshot.Count(item => item.SignedBalance > 0);
        var negativeBalanceCount = accountSnapshot.Count(item => item.SignedBalance < 0);
        var earliestIncludedDate = JournalEntries
            .Where(entry => entry.EntryDate.Date <= periodEndDate.Date)
            .Select(entry => entry.EntryDate.Date)
            .DefaultIfEmpty(periodEndDate.Date)
            .Min();

        AccountStatusRangeSummary = periodStartDate.HasValue
            ? $"{periodLabel} · {periodStartDate.Value:yyyy.MM.dd} - {periodEndDate:yyyy.MM.dd}"
            : $"전체 · {earliestIncludedDate:yyyy.MM.dd} - {periodEndDate:yyyy.MM.dd}";

        AccountStatusHighlights.Clear();
        AccountStatusHighlights.Add(new LedgerMetric("활동 계정", $"{activeAccountCount}개", $"{periodLabel} 안에 움직임이 있는 계정"));
        AccountStatusHighlights.Add(new LedgerMetric("투자 포함", $"{investmentAccountCount}개", "선택 기간 기준 투자 계정 참여 수"));
        AccountStatusHighlights.Add(new LedgerMetric("플러스 잔액", $"{positiveBalanceCount}개", "현재 잔액이 양수인 계정"));
        AccountStatusHighlights.Add(new LedgerMetric("마이너스 잔액", $"{negativeBalanceCount}개", "현재 잔액 보완이 필요한 계정"));

        StatusAccountHighlights.Clear();
        foreach (var accountSummary in accountSnapshot
                     .OrderByDescending(item => item.IncomingTotal + item.OutgoingTotal)
                     .ThenByDescending(item => Math.Abs(item.SignedBalance))
                     .Take(12))
        {
            StatusAccountHighlights.Add(accountSummary);
        }

        AccountTypeSummaries.Clear();
        foreach (var accountType in new[]
                 {
                     AccountType.Asset,
                     AccountType.Investment,
                     AccountType.Liability,
                     AccountType.Expense,
                     AccountType.Revenue,
                     AccountType.Equity
                 })
        {
            var summaries = accountSnapshot.Where(item => item.AccountType == accountType).ToList();
            if (summaries.Count == 0)
            {
                continue;
            }

            AccountTypeSummaries.Add(new AccountTypeBalanceSummary(
                accountType.ToKoreanLabel(),
                summaries.Count,
                summaries.Sum(item => item.SignedBalance),
                GetTypeSummaryDescription(accountType)));
        }
    }

    private void ApplyTheme()
    {
        ThemeService.ApplyTheme(SelectedThemeMode);
    }

    private void RefreshDerivedState()
    {
        RefreshJournalEntryKinds();
        RefreshAccountSummaries();
        RefreshMetrics();
        RefreshMetricCollection();
        RefreshEntryHistoryMonths();
        RefreshAnalysisMonths();
        RefreshAnalysis();
        OnPropertyChanged(nameof(LedgerCountText));
        OnPropertyChanged(nameof(AutoTransferCountText));
        OnPropertyChanged(nameof(ActiveAccounts));
        OnPropertyChanged(nameof(AssetAccounts));
        OnPropertyChanged(nameof(InvestmentAccounts));
        OnPropertyChanged(nameof(LiabilityAccounts));
        OnPropertyChanged(nameof(EquityAccounts));
        OnPropertyChanged(nameof(RevenueAccounts));
        OnPropertyChanged(nameof(ExpenseAccounts));
    }

    private void RefreshJournalEntryKinds()
    {
        foreach (var entry in JournalEntries)
        {
            entry.TransactionKind = ResolveJournalEntryKind(entry);
        }
    }

    private void RefreshMetricCollection()
    {
        Metrics.Clear();
        Metrics.Add(NetAssetMetric);
        Metrics.Add(InvestmentMetric);
        Metrics.Add(LiabilityMetric);
        Metrics.Add(AutoTransferMetric);
    }

    private void RefreshMetrics()
    {
        var netAsset = CalculateNetAssetOn(DateTime.Today);
        var investmentBalance = CalculateCategoryBalanceOn(AccountType.Investment, DateTime.Today);
        var liabilityBalance = CalculateCategoryBalanceOn(AccountType.Liability, DateTime.Today);
        var autoTransferCount = AutoTransferRules.Count;
        var autoTransferTotalAmount = AutoTransferRules.Sum(rule => rule.Amount);

        NetAssetMetric = new LedgerMetric("순자산", $"{netAsset:N0}원", "현재 기준 자산 - 부채");
        InvestmentMetric = new LedgerMetric("투자 잔액", $"{investmentBalance:N0}원", "증권/펀드/코인 등 투자 계정");
        LiabilityMetric = new LedgerMetric("부채 잔액", $"{liabilityBalance:N0}원", "카드/대출 등 상환해야 할 금액");
        AutoTransferMetric = new LedgerMetric("자동이체", $"{autoTransferCount}건", $"총 예정 금액 {autoTransferTotalAmount:N0}원");
    }

    private void RefreshAccountSummaries()
    {
        AccountSummaries.Clear();

        foreach (var accountSummary in BuildAccountSummaries(DateTime.Today))
        {
            AccountSummaries.Add(accountSummary);
        }
    }

    private List<AccountSummary> BuildAccountSummaries(DateTime asOfDate)
    {
        return BuildAccountSummaries(null, asOfDate);
    }

    private List<AccountSummary> BuildAccountSummaries(DateTime? rangeStartDate, DateTime asOfDate)
    {
        var balanceEntries = JournalEntries
            .Where(entry => entry.EntryDate.Date <= asOfDate.Date)
            .ToList();
        var activityEntries = rangeStartDate.HasValue
            ? balanceEntries
                .Where(entry => entry.EntryDate.Date >= rangeStartDate.Value.Date)
                .ToList()
            : balanceEntries;
        var accountSummaries = new List<AccountSummary>();

        foreach (var account in Accounts.Where(item => !item.IsArchived).OrderBy(item => item.Code))
        {
            var incomingTotal = activityEntries
                .Where(entry => entry.ToAccountCode == account.Code)
                .Sum(entry => entry.Amount);

            var outgoingTotal = activityEntries
                .Where(entry => entry.FromAccountCode == account.Code)
                .Sum(entry => entry.Amount);

            if (rangeStartDate.HasValue && incomingTotal == 0m && outgoingTotal == 0m)
            {
                continue;
            }

            var balanceIncomingTotal = balanceEntries
                .Where(entry => entry.ToAccountCode == account.Code)
                .Sum(entry => entry.Amount);

            var balanceOutgoingTotal = balanceEntries
                .Where(entry => entry.FromAccountCode == account.Code)
                .Sum(entry => entry.Amount);

            var signedBalance = CalculateSignedBalance(account.Type, balanceIncomingTotal, balanceOutgoingTotal);

            accountSummaries.Add(new AccountSummary(
                account.Icon,
                account.Name,
                account.Type,
                account.Type.ToKoreanLabel(),
                incomingTotal,
                outgoingTotal,
                signedBalance));
        }

        return accountSummaries;
    }

    private static decimal CalculateSignedBalance(AccountType accountType, decimal incomingTotal, decimal outgoingTotal)
    {
        return accountType switch
        {
            AccountType.Asset => incomingTotal - outgoingTotal,
            AccountType.Investment => incomingTotal - outgoingTotal,
            AccountType.Expense => incomingTotal - outgoingTotal,
            AccountType.Liability => outgoingTotal - incomingTotal,
            AccountType.Equity => outgoingTotal - incomingTotal,
            AccountType.Revenue => outgoingTotal - incomingTotal,
            _ => 0m
        };
    }

    private decimal CalculateNetAssetOn(DateTime date)
    {
        var entries = JournalEntries.Where(entry => entry.EntryDate.Date <= date.Date).ToList();
        var assetTotal = 0m;
        var liabilityTotal = 0m;

        foreach (var account in Accounts)
        {
            var incoming = entries.Where(entry => entry.ToAccountCode == account.Code).Sum(entry => entry.Amount);
            var outgoing = entries.Where(entry => entry.FromAccountCode == account.Code).Sum(entry => entry.Amount);
            var signedBalance = CalculateSignedBalance(account.Type, incoming, outgoing);

            if (account.Type is AccountType.Asset or AccountType.Investment)
            {
                assetTotal += signedBalance;
            }

            if (account.Type == AccountType.Liability)
            {
                liabilityTotal += signedBalance;
            }
        }

        return assetTotal - liabilityTotal;
    }

    private decimal CalculateCategoryBalanceOn(AccountType accountType, DateTime date)
    {
        var entries = JournalEntries.Where(entry => entry.EntryDate.Date <= date.Date).ToList();
        var total = 0m;

        foreach (var account in Accounts.Where(account => !account.IsArchived && account.Type == accountType))
        {
            var incoming = entries.Where(entry => entry.ToAccountCode == account.Code).Sum(entry => entry.Amount);
            var outgoing = entries.Where(entry => entry.FromAccountCode == account.Code).Sum(entry => entry.Amount);
            total += CalculateSignedBalance(account.Type, incoming, outgoing);
        }

        return total;
    }

    private decimal CalculateExpenseOn(DateTime date)
    {
        return JournalEntries
            .Where(entry => entry.EntryDate.Date == date.Date && GetAccountType(entry.ToAccountCode) == AccountType.Expense)
            .Sum(entry => entry.Amount);
    }

    private static string FormatSignedCurrency(decimal amount)
    {
        var sign = amount >= 0 ? "+" : "-";
        return $"{sign}{Math.Abs(amount):N0}원";
    }

    private static double NormalizeBarWidth(decimal amount, decimal maxAmount)
    {
        if (maxAmount <= 0)
        {
            return 0d;
        }

        return (double)(Math.Abs(amount) / maxAmount);
    }

    private static string GetTypeSummaryDescription(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Asset => "현금성 자산과 생활 자금 상태",
            AccountType.Investment => "증권, 펀드, 코인 같은 투자 포지션",
            AccountType.Liability => "카드값이나 대출처럼 상환이 필요한 금액",
            AccountType.Expense => "생활비, 교통비 등 소비성 계정",
            AccountType.Revenue => "급여와 기타 수익 흐름",
            AccountType.Equity => "기초 자본과 누적 순자산 계정",
            _ => string.Empty
        };
    }

    private JournalEntryKind ResolveJournalEntryKind(JournalEntry entry)
    {
        var toType = GetAccountType(entry.ToAccountCode);
        var fromType = GetAccountType(entry.FromAccountCode);

        if (toType == AccountType.Expense)
        {
            return JournalEntryKind.Expense;
        }

        if (fromType == AccountType.Revenue)
        {
            return JournalEntryKind.Revenue;
        }

        return JournalEntryKind.Transfer;
    }

    private static Brush CreateChartBrush(string hex)
    {
        var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
        brush.Freeze();
        return brush;
    }

    private static Geometry BuildDonutSliceGeometry(
        double startAngle,
        double sweepAngle,
        double centerX,
        double centerY,
        double outerRadius,
        double innerRadius)
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            if (sweepAngle >= 359.9d)
            {
                var topOuter = new Point(centerX, centerY - outerRadius);
                var bottomOuter = new Point(centerX, centerY + outerRadius);
                var topInner = new Point(centerX, centerY - innerRadius);
                var bottomInner = new Point(centerX, centerY + innerRadius);

                context.BeginFigure(topOuter, isFilled: true, isClosed: true);
                context.ArcTo(bottomOuter, new Size(outerRadius, outerRadius), 0d, isLargeArc: false, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
                context.ArcTo(topOuter, new Size(outerRadius, outerRadius), 0d, isLargeArc: false, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
                context.LineTo(topInner, isStroked: true, isSmoothJoin: false);
                context.ArcTo(bottomInner, new Size(innerRadius, innerRadius), 0d, isLargeArc: false, SweepDirection.Counterclockwise, isStroked: true, isSmoothJoin: false);
                context.ArcTo(topInner, new Size(innerRadius, innerRadius), 0d, isLargeArc: false, SweepDirection.Counterclockwise, isStroked: true, isSmoothJoin: false);
            }
            else
            {
                var outerStart = PolarToPoint(centerX, centerY, outerRadius, startAngle);
                var outerEnd = PolarToPoint(centerX, centerY, outerRadius, startAngle + sweepAngle);
                var innerStart = PolarToPoint(centerX, centerY, innerRadius, startAngle);
                var innerEnd = PolarToPoint(centerX, centerY, innerRadius, startAngle + sweepAngle);
                var isLargeArc = sweepAngle > 180d;

                context.BeginFigure(outerStart, isFilled: true, isClosed: true);
                context.ArcTo(outerEnd, new Size(outerRadius, outerRadius), 0d, isLargeArc, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: false);
                context.LineTo(innerEnd, isStroked: true, isSmoothJoin: false);
                context.ArcTo(innerStart, new Size(innerRadius, innerRadius), 0d, isLargeArc, SweepDirection.Counterclockwise, isStroked: true, isSmoothJoin: false);
            }
        }

        geometry.Freeze();
        return geometry;
    }

    private static IReadOnlyList<double> BuildVisibleSweepAngles(IReadOnlyList<decimal> amounts, decimal totalAmount)
    {
        if (amounts.Count == 0 || totalAmount <= 0)
        {
            return [];
        }

        if (amounts.Count == 1)
        {
            return [360d];
        }

        const double minimumVisibleSweep = 5d;
        var adjustedSweeps = amounts
            .Select(amount => Math.Max(0.1d, (double)(amount / totalAmount * 360m)))
            .ToArray();

        var overshoot = 0d;
        for (var index = 0; index < adjustedSweeps.Length; index++)
        {
            if (adjustedSweeps[index] < minimumVisibleSweep)
            {
                overshoot += minimumVisibleSweep - adjustedSweeps[index];
                adjustedSweeps[index] = minimumVisibleSweep;
            }
        }

        if (overshoot > 0)
        {
            var donorIndexes = Enumerable.Range(0, adjustedSweeps.Length)
                .OrderByDescending(index => adjustedSweeps[index])
                .ToList();

            foreach (var donorIndex in donorIndexes)
            {
                var available = adjustedSweeps[donorIndex] - minimumVisibleSweep;
                if (available <= 0)
                {
                    continue;
                }

                var donation = Math.Min(available, overshoot);
                adjustedSweeps[donorIndex] -= donation;
                overshoot -= donation;

                if (overshoot <= 0)
                {
                    break;
                }
            }
        }

        var runningTotal = 0d;
        for (var index = 0; index < adjustedSweeps.Length - 1; index++)
        {
            runningTotal += adjustedSweeps[index];
        }

        adjustedSweeps[^1] = Math.Max(0.1d, 360d - runningTotal);
        return adjustedSweeps;
    }

    private static Point PolarToPoint(double centerX, double centerY, double radius, double angleDegrees)
    {
        var radians = Math.PI * angleDegrees / 180d;
        return new Point(
            centerX + radius * Math.Cos(radians),
            centerY + radius * Math.Sin(radians));
    }

    private AccountType? GetAccountType(string code)
    {
        return Accounts.FirstOrDefault(account => account.Code == code)?.Type;
    }

    private PointCollection BuildLinePoints(IReadOnlyList<decimal> values)
    {
        if (values.Count == 0)
        {
            return [];
        }

        if (values.Count == 1)
        {
            return
            [
                new Point(ChartLeftPadding, ChartHeight / 2d),
                new Point(ChartWidth - ChartRightPadding, ChartHeight / 2d)
            ];
        }

        var min = values.Min();
        var max = values.Max();
        var range = max - min;
        if (range == 0)
        {
            range = 1;
        }

        var drawableHeight = ChartHeight - ChartTopPadding - ChartBottomPadding;
        var drawableWidth = ChartWidth - ChartLeftPadding - ChartRightPadding;
        var points = new PointCollection(values.Count);
        for (var index = 0; index < values.Count; index++)
        {
            var x = ChartLeftPadding + drawableWidth * index / (values.Count - 1d);
            var y = ChartTopPadding + (double)((max - values[index]) / range) * drawableHeight;
            points.Add(new Point(x, y));
        }

        return points;
    }

    private void RefreshChartPoints(
        ObservableCollection<AnalysisChartPoint> target,
        IReadOnlyList<DateTime> dates,
        IReadOnlyList<decimal> values)
    {
        target.Clear();

        if (dates.Count == 0 || values.Count == 0 || dates.Count != values.Count)
        {
            return;
        }

        if (values.Count == 1)
        {
            target.Add(new AnalysisChartPoint(ChartWidth / 2d, ChartHeight / 2d, dates[0], values[0]));
            return;
        }

        var visibleIndexes = GetVisibleChartPointIndexes(values);

        var min = values.Min();
        var max = values.Max();
        var range = max - min;
        if (range == 0)
        {
            range = 1;
        }

        var drawableHeight = ChartHeight - ChartTopPadding - ChartBottomPadding;
        var drawableWidth = ChartWidth - ChartLeftPadding - ChartRightPadding;
        foreach (var index in visibleIndexes)
        {
            var x = ChartLeftPadding + drawableWidth * index / (values.Count - 1d);
            var y = ChartTopPadding + (double)((max - values[index]) / range) * drawableHeight;
            target.Add(new AnalysisChartPoint(x, y, dates[index], values[index]));
        }
    }

    private void RefreshAnalysisLabels(IReadOnlyList<DateTime> dates)
    {
        AnalysisLabels.Clear();
        if (dates.Count == 0)
        {
            return;
        }

        var indexes = new HashSet<int> { 0, dates.Count / 2, dates.Count - 1 };
        var drawableWidth = ChartWidth - ChartLeftPadding - ChartRightPadding;
        foreach (var index in indexes.OrderBy(value => value))
        {
            var left = dates.Count == 1
                ? ChartWidth / 2d
                : ChartLeftPadding + drawableWidth * index / (dates.Count - 1d);
            AnalysisLabels.Add(new ChartLabel(left, dates[index].ToString("MM.dd")));
        }
    }

    private static IReadOnlyList<int> GetVisibleChartPointIndexes(IReadOnlyList<decimal> values)
    {
        if (values.Count == 0)
        {
            return [];
        }

        if (values.Count == 1)
        {
            return [0];
        }

        var indexes = new List<int> { 0 };
        for (var index = 1; index < values.Count - 1; index++)
        {
            if (values[index] != values[index - 1] || values[index] != values[index + 1])
            {
                indexes.Add(index);
            }
        }

        if (indexes[^1] != values.Count - 1)
        {
            indexes.Add(values.Count - 1);
        }

        return indexes;
    }

    private IEnumerable<DateTime> EachDay(DateTime start, DateTime end)
    {
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            yield return date;
        }
    }

    private IEnumerable<AccountDefinition> GetAccountsByType(AccountType type)
    {
        return Accounts
            .Where(account => !account.IsArchived && account.Type == type)
            .OrderBy(account => account.Code)
            .ToList();
    }

    private string GenerateNextCode(AccountType type)
    {
        var baseCode = type switch
        {
            AccountType.Asset => 1000,
            AccountType.Investment => 1500,
            AccountType.Liability => 2000,
            AccountType.Equity => 3000,
            AccountType.Revenue => 4000,
            AccountType.Expense => 5000,
            _ => 9000
        };

        var maxCode = Accounts
            .Where(account => account.Type == type)
            .Select(account => int.TryParse(account.Code, out var code) ? code : baseCode)
            .DefaultIfEmpty(baseCode - 10)
            .Max();

        return (Math.Max(baseCode, maxCode + 10)).ToString(CultureInfo.InvariantCulture);
    }

    private static string GenerateIcon(AccountType type)
    {
        return type switch
        {
            AccountType.Asset => "자",
            AccountType.Investment => "투",
            AccountType.Liability => "부",
            AccountType.Equity => "순",
            AccountType.Revenue => "수",
            AccountType.Expense => "비",
            _ => "계"
        };
    }

    private static string BuildAvatarText(string displayName)
    {
        var trimmed = string.Concat(displayName.Where(character => !char.IsWhiteSpace(character)));
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return "ME";
        }

        return trimmed.Length == 1 ? trimmed : trimmed[..2];
    }

    private void SeedAccounts()
    {
        var defaultAccounts = new[]
        {
            new AccountDefinition("1000", "현금", "지갑에 있는 현금", AccountType.Asset, "자"),
            new AccountDefinition("1010", "입출금통장", "주거래 통장", AccountType.Asset, "자"),
            new AccountDefinition("1020", "비상금통장", "예비 자금", AccountType.Asset, "자"),
            new AccountDefinition("1500", "증권계좌", "주식과 ETF를 관리하는 투자 계정", AccountType.Investment, "투"),
            new AccountDefinition("2000", "신용카드", "카드 결제 대금", AccountType.Liability, "부"),
            new AccountDefinition("3000", "순자산", "기초 순자산", AccountType.Equity, "순"),
            new AccountDefinition("4000", "급여", "월급 수입", AccountType.Revenue, "수"),
            new AccountDefinition("4010", "기타수익", "부수입", AccountType.Revenue, "수"),
            new AccountDefinition("5000", "생활비", "일상 지출", AccountType.Expense, "비"),
            new AccountDefinition("5010", "교통비", "버스와 지하철", AccountType.Expense, "비"),
            new AccountDefinition("5020", "주거비", "월세와 관리비", AccountType.Expense, "비")
        };

        foreach (var account in defaultAccounts)
        {
            Accounts.Add(account);
        }
    }

    private void SeedSampleEntries()
    {
        InsertEntrySorted(new JournalEntry(DateTime.Today.AddDays(-24), "급여 입금", "1010", "입출금통장", "자", "4000", "급여", "수", 3200000m, "월급 수령"));
        InsertEntrySorted(new JournalEntry(DateTime.Today.AddDays(-18), "주거비 이체", "5020", "주거비", "비", "1010", "입출금통장", "자", 820000m, "월세"));
        InsertEntrySorted(new JournalEntry(DateTime.Today.AddDays(-13), "생활비 인출", "1000", "현금", "자", "1010", "입출금통장", "자", 180000m, "주간 현금"));
        InsertEntrySorted(new JournalEntry(DateTime.Today.AddDays(-11), "ETF 매수", "1500", "증권계좌", "투", "1010", "입출금통장", "자", 450000m, "월간 투자"));
        InsertEntrySorted(new JournalEntry(DateTime.Today.AddDays(-9), "간식 구매", "5000", "생활비", "비", "1000", "현금", "자", 6800m, "카페"));
        InsertEntrySorted(new JournalEntry(DateTime.Today.AddDays(-6), "대중교통", "5010", "교통비", "비", "1000", "현금", "자", 3250m, "버스 요금"));
        InsertEntrySorted(new JournalEntry(DateTime.Today.AddDays(-3), "비상금 이동", "1020", "비상금통장", "자", "1010", "입출금통장", "자", 300000m, "예비 자금"));
        InsertEntrySorted(new JournalEntry(DateTime.Today.AddDays(-1), "카드값 반영", "2000", "신용카드", "부", "1010", "입출금통장", "자", 410000m, "자동 이체"));
    }

    private void ResetDemoData()
    {
        if (JournalEntries.Count == 0)
        {
            MessageBox.Show("삭제할 거래가 없습니다.", "데모 데이터 삭제", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            "현재 입력된 샘플 거래와 기록을 모두 삭제합니다. 계정 설정은 유지됩니다. 계속할까요?",
            "데모 데이터 삭제",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        JournalEntries.Clear();
        CurrentManagedAccount = null;

        if (IsAccountPopupOpen)
        {
            CloseAccountPopup();
        }

        if (IsUserProfilePopupOpen)
        {
            CloseUserProfilePopup();
        }

        EditingJournalEntry = null;
        ResetEditEntryForm();
        IsEntryEditTabSelected = false;
        ResetEntryForm();
        RefreshDerivedState();
    }

    private void InsertEntrySorted(JournalEntry entry)
    {
        var index = 0;
        while (index < JournalEntries.Count && JournalEntries[index].EntryDate >= entry.EntryDate)
        {
            index++;
        }

        JournalEntries.Insert(index, entry);
    }

    private void InsertAutoTransferSorted(AutoTransferRule rule)
    {
        var index = 0;
        while (index < AutoTransferRules.Count &&
               (AutoTransferRules[index].TransferDay < rule.TransferDay ||
                AutoTransferRules[index].TransferDay == rule.TransferDay &&
                string.Compare(AutoTransferRules[index].Description, rule.Description, StringComparison.CurrentCultureIgnoreCase) <= 0))
        {
            index++;
        }

        AutoTransferRules.Insert(index, rule);
    }

    private void UpdateJournalEntry(
        JournalEntry entry,
        DateTime entryDate,
        string description,
        AccountDefinition fromAccount,
        AccountDefinition toAccount,
        decimal amount,
        string memo)
    {
        JournalEntries.Remove(entry);
        entry.EntryDate = entryDate;
        entry.Description = description;
        entry.FromAccountCode = fromAccount.Code;
        entry.FromAccountName = fromAccount.Name;
        entry.FromAccountIcon = fromAccount.Icon;
        entry.ToAccountCode = toAccount.Code;
        entry.ToAccountName = toAccount.Name;
        entry.ToAccountIcon = toAccount.Icon;
        entry.Amount = amount;
        entry.Memo = memo;
        InsertEntrySorted(entry);
    }

    private void UpdateAutoTransferRule(
        AutoTransferRule rule,
        string description,
        AccountDefinition fromAccount,
        AccountDefinition toAccount,
        decimal amount,
        string memo,
        int transferDay)
    {
        AutoTransferRules.Remove(rule);
        rule.Description = description;
        rule.FromAccountCode = fromAccount.Code;
        rule.FromAccountName = fromAccount.Name;
        rule.FromAccountIcon = fromAccount.Icon;
        rule.ToAccountCode = toAccount.Code;
        rule.ToAccountName = toAccount.Name;
        rule.ToAccountIcon = toAccount.Icon;
        rule.Amount = amount;
        rule.Memo = memo;
        rule.TransferDay = transferDay;
        InsertAutoTransferSorted(rule);
    }

    private void RefreshAccountSelections()
    {
        SelectedFromAccount = ResolveActiveAccount(SelectedFromAccount?.Code, GetDefaultFromAccount());
        SelectedToAccount = ResolveActiveAccount(SelectedToAccount?.Code, GetDefaultToAccount(SelectedFromAccount?.Code));
        EditSelectedFromAccount = ResolveActiveAccount(EditSelectedFromAccount?.Code, GetDefaultFromAccount());
        EditSelectedToAccount = ResolveActiveAccount(EditSelectedToAccount?.Code, GetDefaultToAccount(EditSelectedFromAccount?.Code));
        SelectedAutoTransferFromAccount = ResolveActiveAccount(SelectedAutoTransferFromAccount?.Code, GetDefaultFromAccount());
        SelectedAutoTransferToAccount = ResolveActiveAccount(SelectedAutoTransferToAccount?.Code, GetDefaultToAccount(SelectedAutoTransferFromAccount?.Code));

        if (SelectedFromAccount is not null && SelectedToAccount is not null && SelectedFromAccount.Code == SelectedToAccount.Code)
        {
            SelectedToAccount = GetDefaultToAccount(SelectedFromAccount.Code);
        }

        if (EditSelectedFromAccount is not null && EditSelectedToAccount is not null && EditSelectedFromAccount.Code == EditSelectedToAccount.Code)
        {
            EditSelectedToAccount = GetDefaultToAccount(EditSelectedFromAccount.Code);
        }

        if (SelectedAutoTransferFromAccount is not null &&
            SelectedAutoTransferToAccount is not null &&
            SelectedAutoTransferFromAccount.Code == SelectedAutoTransferToAccount.Code)
        {
            SelectedAutoTransferToAccount = GetDefaultToAccount(SelectedAutoTransferFromAccount.Code);
        }
    }

    private AccountDefinition? ResolveActiveAccount(string? code, AccountDefinition? fallback)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            var matchedAccount = Accounts.FirstOrDefault(account => !account.IsArchived && account.Code == code);
            if (matchedAccount is not null)
            {
                return matchedAccount;
            }
        }

        return fallback;
    }

    private AccountDefinition? GetDefaultFromAccount()
    {
        return Accounts.FirstOrDefault(account => !account.IsArchived && account.Code == "1010")
            ?? Accounts.FirstOrDefault(account => !account.IsArchived && account.Type == AccountType.Asset)
            ?? Accounts.FirstOrDefault(account => !account.IsArchived);
    }

    private AccountDefinition? GetDefaultToAccount(string? exceptCode = null)
    {
        var preferredExpense = Accounts.FirstOrDefault(account => !account.IsArchived && account.Code == "5000" && account.Code != exceptCode);
        if (preferredExpense is not null)
        {
            return preferredExpense;
        }

        return Accounts.FirstOrDefault(account => !account.IsArchived && account.Type == AccountType.Expense && account.Code != exceptCode)
            ?? Accounts.FirstOrDefault(account => !account.IsArchived && account.Code != exceptCode);
    }

    private void UpdateJournalEntriesForAccount(AccountDefinition account)
    {
        foreach (var entry in JournalEntries.Where(entry => entry.FromAccountCode == account.Code))
        {
            entry.FromAccountName = account.Name;
            entry.FromAccountIcon = account.Icon;
        }

        foreach (var entry in JournalEntries.Where(entry => entry.ToAccountCode == account.Code))
        {
            entry.ToAccountName = account.Name;
            entry.ToAccountIcon = account.Icon;
        }
    }

    private void UpdateAutoTransferRulesForAccount(AccountDefinition account)
    {
        foreach (var rule in AutoTransferRules.Where(rule => rule.FromAccountCode == account.Code))
        {
            rule.FromAccountName = account.Name;
            rule.FromAccountIcon = account.Icon;
        }

        foreach (var rule in AutoTransferRules.Where(rule => rule.ToAccountCode == account.Code))
        {
            rule.ToAccountName = account.Name;
            rule.ToAccountIcon = account.Icon;
        }
    }

    private void ResetEntryForm()
    {
        EntryDate = DateTime.Today;
        Description = string.Empty;
        AmountText = string.Empty;
        Memo = string.Empty;
        SelectedFromAccount = GetDefaultFromAccount();
        SelectedToAccount = GetDefaultToAccount(SelectedFromAccount?.Code);
        ValidationMessage = string.Empty;
    }

    private void ResetEditEntryForm()
    {
        EditEntryDate = DateTime.Today;
        EditDescription = string.Empty;
        EditAmountText = string.Empty;
        EditMemo = string.Empty;
        EditSelectedFromAccount = GetDefaultFromAccount();
        EditSelectedToAccount = GetDefaultToAccount(EditSelectedFromAccount?.Code);
        EditValidationMessage = string.Empty;
    }

    private void ResetAutoTransferForm()
    {
        EditingAutoTransferRule = null;
        SelectedAutoTransferDay = AutoTransferDayOptions.Contains(25) ? 25 : AutoTransferDayOptions.FirstOrDefault();
        AutoTransferDescription = string.Empty;
        AutoTransferAmountText = string.Empty;
        AutoTransferMemo = string.Empty;
        SelectedAutoTransferFromAccount = GetDefaultFromAccount();
        SelectedAutoTransferToAccount = GetDefaultToAccount(SelectedAutoTransferFromAccount?.Code);
        AutoTransferValidationMessage = string.Empty;
    }

    private void CancelJournalEntryEdit()
    {
        EditingJournalEntry = null;
        ResetEditEntryForm();
        IsEntryEditTabSelected = false;
        RefreshDerivedState();
    }

    private void CancelAutoTransferEdit()
    {
        ResetAutoTransferForm();
        RefreshDerivedState();
    }

    private void SeedAutoTransferDayOptions()
    {
        AutoTransferDayOptions.Clear();
        for (var day = 1; day <= 31; day++)
        {
            AutoTransferDayOptions.Add(day);
        }
    }

    private void SeedSampleAutoTransfers()
    {
        var bankAccount = Accounts.FirstOrDefault(account => account.Code == "1010");
        var housingExpense = Accounts.FirstOrDefault(account => account.Code == "5020");
        var investmentAccount = Accounts.FirstOrDefault(account => account.Code == "1500");
        var creditCard = Accounts.FirstOrDefault(account => account.Code == "2000");

        if (bankAccount is null)
        {
            return;
        }

        if (housingExpense is not null)
        {
            InsertAutoTransferSorted(new AutoTransferRule(
                Guid.NewGuid().ToString("N"),
                "월세 자동이체",
                housingExpense.Code,
                housingExpense.Name,
                housingExpense.Icon,
                bankAccount.Code,
                bankAccount.Name,
                bankAccount.Icon,
                820000m,
                "매월 주거비",
                5));
        }

        if (investmentAccount is not null)
        {
            InsertAutoTransferSorted(new AutoTransferRule(
                Guid.NewGuid().ToString("N"),
                "월간 투자",
                investmentAccount.Code,
                investmentAccount.Name,
                investmentAccount.Icon,
                bankAccount.Code,
                bankAccount.Name,
                bankAccount.Icon,
                450000m,
                "ETF 정기 매수",
                11));
        }

        if (creditCard is not null)
        {
            InsertAutoTransferSorted(new AutoTransferRule(
                Guid.NewGuid().ToString("N"),
                "카드 대금 결제",
                creditCard.Code,
                creditCard.Name,
                creditCard.Icon,
                bankAccount.Code,
                bankAccount.Name,
                bankAccount.Icon,
                410000m,
                "카드값 납부",
                23));
        }
    }

    private sealed class AccountBookBackup
    {
        public int Version { get; set; } = 1;

        public DateTime ExportedAt { get; set; }

        public AppThemeMode ThemeMode { get; set; } = AppThemeMode.Dark;

        public string UserProfileName { get; set; } = "사용자";

        public string LedgerName { get; set; } = "개인 장부";

        public string UserProfileNote { get; set; } = "내 장부 정보를 관리합니다.";

        public List<AccountBackupItem> Accounts { get; set; } = [];

        public List<JournalEntryBackupItem> JournalEntries { get; set; } = [];

        public List<AutoTransferRuleBackupItem> AutoTransferRules { get; set; } = [];
    }

    private sealed class AccountBackupItem
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public AccountType Type { get; set; }

        public string Icon { get; set; } = string.Empty;

        public bool IsArchived { get; set; }
    }

    private sealed class JournalEntryBackupItem
    {
        public DateTime EntryDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public string ToAccountCode { get; set; } = string.Empty;

        public string ToAccountName { get; set; } = string.Empty;

        public string ToAccountIcon { get; set; } = string.Empty;

        public string FromAccountCode { get; set; } = string.Empty;

        public string FromAccountName { get; set; } = string.Empty;

        public string FromAccountIcon { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Memo { get; set; } = string.Empty;
    }

    private sealed class AutoTransferRuleBackupItem
    {
        public string Id { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ToAccountCode { get; set; } = string.Empty;

        public string ToAccountName { get; set; } = string.Empty;

        public string ToAccountIcon { get; set; } = string.Empty;

        public string FromAccountCode { get; set; } = string.Empty;

        public string FromAccountName { get; set; } = string.Empty;

        public string FromAccountIcon { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Memo { get; set; } = string.Empty;

        public int TransferDay { get; set; } = 1;
    }
}

public enum AppSection
{
    Entry,
    Analysis,
    Settings
}
