using MSAVA_App.Presentation.Login;
using MSAVA_App.Presentation.Welcome;
using MSAVA_App.Services.Navigation;
using MSAVA_App.Services.Session;
using Uno.Extensions.Authentication;
using Uno.Extensions.Navigation;

namespace MSAVA_App.Presentation;

public class ShellModel
{
    private readonly NavigationService _navigation;
    private readonly IAuthenticationService _auth;
    private bool _initialized;

    public ShellModel(
        LocalSessionService localSession,
        INavigator navigator,
        NavigationService navigation,
        IAuthenticationService auth)
    {
        _navigation = navigation;
        _auth = auth;
        _navigation.SetNavigator(navigator);
        _navigation.SetRootOwner(this);
        localSession.LoggedOut += OnLoggedOut;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        var authenticated = await _auth.RefreshAsync();
        if (authenticated)
        {
            await _navigation.NavigateTo<MainModel>(this, qualifier: Qualifiers.Nested);
        }
        else
        {
            await _navigation.NavigateTo<LoginModel>(this, qualifier: Qualifiers.Nested);
        }
    }

    private async void OnLoggedOut(object? sender, EventArgs e)
    {
        // Centralized navigation to Login using NavigationService
        await _navigation.NavigateTo<LoginModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
