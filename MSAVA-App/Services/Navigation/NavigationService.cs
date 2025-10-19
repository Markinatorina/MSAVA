using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MSAVA_App.Presentation.Login;
using Uno.Extensions.Navigation;

namespace MSAVA_App.Services.Navigation;

internal class NavigationService
{
    private static readonly ConcurrentDictionary<Type, NavigationServiceOptions> _options = new();
    private readonly IAuthenticationService _auth;

    public NavigationService(IAuthenticationService auth)
    {
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
    }

    public static void RegisterFor<TViewModel>(NavigationServiceOptions options) where TViewModel : class
        => _options[typeof(TViewModel)] = options ?? new NavigationServiceOptions();

    public static NavigationServiceOptions GetOptionsFor<TViewModel>() where TViewModel : class
        => _options.TryGetValue(typeof(TViewModel), out var opts) ? opts : new NavigationServiceOptions();

    public async Task<bool> EnsureAccessAsync<TViewModel>(CancellationToken ct = default) where TViewModel : class
    {
        var opts = GetOptionsFor<TViewModel>();
        if (opts.Public)
            return true;

        // Protected: require authenticated session
        var isAuthenticated = await _auth.RefreshAsync(ct);
        return isAuthenticated;
    }

    public async Task<bool> NavigateViewModelAsync<TViewModel>(object owner,
        INavigator navigator,
        string? qualifier = null,
        object? data = null,
        CancellationToken ct = default) where TViewModel : class
    {
        if (navigator is null) throw new ArgumentNullException(nameof(navigator));

        var allowed = await EnsureAccessAsync<TViewModel>(ct);
        if (!allowed)
        {
            // Redirect to Login and clear backstack when blocked
            await navigator.NavigateViewModelAsync<LoginModel>(owner, qualifier: Qualifiers.ClearBackStack, cancellation: ct);
            return false;
        }

        await navigator.NavigateViewModelAsync<TViewModel>(owner, qualifier: qualifier, data: data, cancellation: ct);
        return true;
    }
}
