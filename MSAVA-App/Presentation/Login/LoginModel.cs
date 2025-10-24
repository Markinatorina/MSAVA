using MSAVA_App.Presentation.Welcome;
using MSAVA_App.Services.Navigation;
using MSAVA_App.Services.Session;

namespace MSAVA_App.Presentation.Login;

public partial record LoginModel(IDispatcher Dispatcher, INavigator Navigator, IAuthenticationService Authentication, NavigationService Navigation, LocalSessionService LocalSession)
{
    public string Title { get; } = "Login";

    public IState<string> Username => State<string>.Value(this, () => string.Empty);

    public IState<string> Password => State<string>.Value(this, () => string.Empty);

    public async ValueTask Login(CancellationToken token)
    {
        var username = await Username ?? string.Empty;
        var password = await Password ?? string.Empty;

        var tokenStr = await LocalSession.LoginAsync(username, password, token);
        if (!string.IsNullOrWhiteSpace(tokenStr))
        {
            await Navigation.NavigateTo<MainModel>(this, qualifier: Qualifiers.ClearBackStack, ct: token);
        }
    }
}
