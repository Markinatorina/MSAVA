using MSAVA_App.Presentation.Login;
using MSAVA_App.Services.Navigation;
using MSAVA_App.Services.Session;

namespace MSAVA_App.Presentation;

public class ShellModel
{
    private readonly NavigationService _navigation;

    public ShellModel(
        LocalSessionService localSession,
        INavigator navigator,
        NavigationService navigation)
    {
        _navigation = navigation;
        _navigation.SetNavigator(navigator);
        localSession.LoggedOut += OnLoggedOut;
    }

    private async void OnLoggedOut(object? sender, EventArgs e)
    {
        // Centralized navigation to Login using NavigationService
        await _navigation.NavigateViewModelAsync<LoginModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
