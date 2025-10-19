using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Extensions.Navigation;
using MSAVA_App.Models;
using MSAVA_App.Presentation.Login;
using MSAVA_App.Presentation.Welcome;
using MSAVA_App.Services.Navigation;

namespace MSAVA_App.Presentation;

// Registering new pages/routes
// 1) Create your XAML Page and its ViewModel (e.g., MyPage.xaml + MyModel).
// 2) In Register(...), add a mapping to views.Register:
//    - Basic page: new ViewMap<MyPage, MyModel>()
//    - Page with typed navigation data: new DataViewMap<MyPage, MyModel, MyData>()
// 3) Register guard options for your ViewModel using NavigationService:
//    - Public route (accessible offline/unauthenticated): NavigationService.RegisterFor<MyModel>(new NavigationServiceOptions { Public = true });
//    - Protected route (requires authentication):        NavigationService.RegisterFor<MyModel>(new NavigationServiceOptions { Public = false });
// 4) In Register(...), add a route entry to the local rootChildren list:
//    - new("MyPath", typeof(MyModel))
//    - Make it default by adding IsDefault: true
// 5) Navigate:
//    - Direct: await _navigator.NavigateViewModelAsync<MyModel>(this, qualifier: Qualifiers.Nested, data: myData);
//    - Guarded (recommended): await _navigationService.NavigateViewModelAsync<MyModel>(this, _navigator, qualifier: Qualifiers.Nested, data: myData);
// Notes:
// - All routes are nested under the Shell root and render in Shell.xaml’s region.
// - Changing the default route affects the initial content after authentication.
// - Keep ViewModel types unique per route.
internal static class Routes
{
    private sealed record Node(string Path, Type ViewModel, bool IsDefault = false);

    public static void Register(IViewRegistry views, IRouteRegistry routes)
    {
        // Register views with their ViewModels
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<LoginPage, LoginModel>(),
            new ViewMap<MainPage, MainModel>(),
            new DataViewMap<SecondPage, SecondModel, Entity>()
        );

        // Register navigation guards/options per route
        NavigationService.RegisterFor<LoginModel>(new NavigationServiceOptions { Public = true });
        NavigationService.RegisterFor<MainModel>(new NavigationServiceOptions { Public = false });
        NavigationService.RegisterFor<SecondModel>(new NavigationServiceOptions { Public = false });

        // Build lookup for route construction from registered maps
        var byViewModel = new Dictionary<Type, ViewMap>
        {
            [typeof(ShellModel)] = views.FindByViewModel<ShellModel>(),
            [typeof(LoginModel)] = views.FindByViewModel<LoginModel>(),
            [typeof(MainModel)] = views.FindByViewModel<MainModel>(),
            [typeof(SecondModel)] = views.FindByViewModel<SecondModel>()
        };

        // Define routes nested under Shell
        Node[] rootChildren =
        [
            new("Login", typeof(LoginModel)),
            new("Main", typeof(MainModel), IsDefault: true),
            new("Second", typeof(SecondModel))
        ];

        // Build root route with nested children
        var children = rootChildren
            .Select(n => new RouteMap(n.Path, View: byViewModel[n.ViewModel], IsDefault: n.IsDefault))
            .ToArray();

        var root = new RouteMap("", View: byViewModel[typeof(ShellModel)], Nested: [ .. children ]);

        routes.Register(root);
    }
}
