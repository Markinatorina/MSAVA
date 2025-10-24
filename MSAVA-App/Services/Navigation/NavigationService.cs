using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MSAVA_App.Presentation.Login;
using MSAVA_App.Services.Session;
using Uno.Extensions.Navigation;

namespace MSAVA_App.Services.Navigation;

public class NavigationService
{
    private static readonly ConcurrentDictionary<Type, NavigationServiceOptions> _options = new();
    private readonly LocalSessionService _session;
    private INavigator? _navigator;

    public NavigationService(LocalSessionService session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public void SetNavigator(INavigator navigator)
        => _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));

    public static void RegisterFor<TViewModel>(NavigationServiceOptions options) where TViewModel : class
        => _options[typeof(TViewModel)] = options ?? new NavigationServiceOptions();

    public static NavigationServiceOptions GetOptionsFor<TViewModel>() where TViewModel : class
        => _options.TryGetValue(typeof(TViewModel), out var opts) ? opts : new NavigationServiceOptions();

    public Task<bool> EnsureAccessAsync<TViewModel>(CancellationToken ct = default) where TViewModel : class
    {
        var opts = GetOptionsFor<TViewModel>();
        if (opts.Public)
            return Task.FromResult(true);

        return Task.FromResult(_session.IsLoggedIn);
    }

    public async Task<bool> NavigateViewModelAsync<TViewModel>(object owner,
        string? qualifier = null,
        object? data = null,
        CancellationToken ct = default) where TViewModel : class
    {
        var navigator = _navigator ?? throw new InvalidOperationException("Navigator not initialized. Call NavigationService.SetNavigator at app startup (e.g., from ShellModel).");

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
