using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace MSAVA_App.Presentation.FileManagement;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FileManagementPage : Page
{
    private bool _loadedOnce;
    private FileManagementModel? _vm;

    public FileManagementPage()
    {
        this.InitializeComponent();
        this.Loaded += FileManagementPage_Loaded;
        this.DataContextChanged += FileManagementPage_DataContextChanged;
    }

    private static FileManagementModel? ResolveVm(object? dc)
        => dc as FileManagementModel
           ?? (dc?.GetType().GetProperty("Model")?.GetValue(dc) as FileManagementModel)
           ?? (dc?.GetType().GetProperty("ViewModel")?.GetValue(dc) as FileManagementModel);

    private async void FileManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        // If VM is already present by the time Loaded fires, trigger once.
        if (_loadedOnce) return;
        var vm = ResolveVm(DataContext);
        if (vm is not null)
        {
            _vm = vm;
            _loadedOnce = true;
            await vm.SaveAndRefreshAsync();
        }
    }

    private async void FileManagementPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // In some platforms DataContext is set after Loaded. Trigger once when VM arrives.
        if (_loadedOnce) return;
        var vm = ResolveVm(args.NewValue);
        if (vm is not null)
        {
            _vm = vm;
            _loadedOnce = true;
            await vm.SaveAndRefreshAsync();
        }
    }

    private void FilesNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        Console.WriteLine("Invoked: " + args.InvokedItem);
        // Prefer cached VM because DataContext can be different for NavigationView
        var vm = _vm ?? ResolveVm(DataContext) ?? ResolveVm(sender.DataContext);
        Console.WriteLine("VM present: " + (vm is not null));
        if (vm is null) return;

        if (args.IsSettingsInvoked)
        {
            Console.WriteLine("Settings invoked");
            return;
        }

        var tag = (args.InvokedItemContainer as NavigationViewItem)?.Tag as string;
        switch (tag)
        {
            case "Exit":
                _ = vm.GoToMainAsync();
                break;
            case "YourFiles":
                break;
        }
    }
}
