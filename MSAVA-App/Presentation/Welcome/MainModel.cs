using MSAVA_App.Presentation.FileManagement;
using MSAVA_App.Services.Session;
using MSAVA_App.Services.Navigation;

namespace MSAVA_App.Presentation.Welcome;

public partial record MainModel
{
    private readonly LocalSessionService _session;
    private readonly NavigationService _navigation;

    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        LocalSessionService session,
        NavigationService navigation)
    {
        _session = session;
        _navigation = navigation;
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public string Username => _session.CurrentSession?.Username ?? string.Empty;

    public string WelcomeText => string.IsNullOrWhiteSpace(Username) ? string.Empty : $"Welcome, {Username.Trim()}.";

    public async Task GoToFiles()
    {
        await _navigation.NavigateTo<FileManagementModel>(this);
    }

    public ValueTask Logout(CancellationToken token)
    {
        _session.Logout();
        return ValueTask.CompletedTask;
    }
}
