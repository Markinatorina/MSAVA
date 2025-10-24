using MSAVA_App.Presentation.FileManagement;
using MSAVA_App.Services.Authentication;

namespace MSAVA_App.Presentation.Welcome;

public partial record MainModel
{
    private INavigator _navigator;
    private readonly AuthenticationService _authService;

    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        IAuthenticationService authentication,
        INavigator navigator,
        AuthenticationService authService)
    {
        _navigator = navigator;
        _authentication = authentication;
        _authService = authService;
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public string Username => _authService.CurrentSession?.Username ?? string.Empty;

    public string WelcomeText => string.IsNullOrWhiteSpace(Username) ? string.Empty : $"Welcome, {Username.Trim()}.";

    public async Task GoToFiles()
    {
        await _navigator.NavigateViewModelAsync<FileManagementModel>(this);
    }

    public async ValueTask Logout(CancellationToken token)
    {
        await _authentication.LogoutAsync(token);
    }

    private IAuthenticationService _authentication;
}
