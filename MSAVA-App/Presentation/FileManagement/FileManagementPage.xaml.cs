using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System.Threading.Tasks;
using System.IO;
using MSAVA_App.Services.Pickers;

namespace MSAVA_App.Presentation.FileManagement;

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
        // Prefer cached VM because DataContext can be different for NavigationView
        var vm = _vm ?? ResolveVm(DataContext) ?? ResolveVm(sender.DataContext);
        if (vm is null) return;

        var tag = (args.InvokedItemContainer as NavigationViewItem)?.Tag as string;
        switch (tag)
        {
            case "Exit":
                _ = vm.GoToMainAsync();
                break;
            case "YourFiles":
                vm.IsAddMode = false;
                break;
            case "AddFiles":
                _ = vm.StartAddAsync();
                break;
        }
    }

    private async void Upload_Click(object sender, RoutedEventArgs e)
    {
        var vm = _vm ?? ResolveVm(DataContext);
        if (vm is null) return;
        await vm.UploadAsync();

        // Show dialog when VM indicates a result is available
        if (vm.ShowUploadResult)
        {
            UploadResultDialog.DataContext = vm; // ensure bindings resolve
            UploadResultDialog.XamlRoot = this.XamlRoot; // ensure dialog attaches to this visual tree
            await UploadResultDialog.ShowAsync();
        }
    }

    private void UploadResultDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var vm = _vm ?? ResolveVm(DataContext);
        if (vm is null) return;
        var text = vm.UploadResultCopyText ?? string.Empty;
        try
        {
            var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dp.SetText(text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }
        catch
        {
            // ignore clipboard failures on some platforms
        }
    }

    private void UploadResultDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        var vm = _vm ?? ResolveVm(DataContext);
        if (vm is null) return;
        vm.ShowUploadResult = false;
    }

    private async void PickFile_Click(object sender, RoutedEventArgs e)
    {
        var vm = _vm ?? ResolveVm(DataContext);
        if (vm is null) return;

        var picked = await FilePicker.PickSingleAsync();
        if (picked is null) return;

        // If extension/name not set, default from picked file
        if (string.IsNullOrWhiteSpace(vm.NewFileName))
        {
            vm.NewFileName = picked.FileName;
        }
        if (string.IsNullOrWhiteSpace(vm.NewFileExtension))
        {
            vm.NewFileExtension = picked.Extension;
        }

        vm.NewFileStream = picked.Stream;
    }
}

internal static class ObjectExtensions
{
    public static T Also<T>(this T obj, System.Action<T> action)
    {
        action(obj);
        return obj;
    }
}
