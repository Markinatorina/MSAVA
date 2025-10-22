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

    public FileManagementPage()
    {
        this.InitializeComponent();
        this.Loaded += FileManagementPage_Loaded;
        this.DataContextChanged += FileManagementPage_DataContextChanged;
    }

    private async void FileManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        // If VM is already present by the time Loaded fires, trigger once.
        if (_loadedOnce) return;
        if (DataContext is FileManagementModel vm)
        {
            _loadedOnce = true;
            await vm.LoadAsync();
        }
    }

    private async void FileManagementPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // In some platforms DataContext is set after Loaded. Trigger once when VM arrives.
        if (_loadedOnce) return;
        if (args.NewValue is FileManagementModel vm)
        {
            _loadedOnce = true;
            await vm.LoadAsync();
        }
    }
}
