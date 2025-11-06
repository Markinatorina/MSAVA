using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace MSAVA_App.Services.Pickers;

public sealed class PickedFile
{
    public required Stream Stream { get; init; }
    public required string FileName { get; init; }
    public required string Extension { get; init; }
}

// Helper for picking files from the system file picker
// Purpose of this wrapper is to isolate platform-specific code related to FileOpenPicker and centralize logic
public static class FilePicker
{
    public static async Task<PickedFile?> PickSingleAsync()
    {
        var picker = CreatePicker();
        picker.FileTypeFilter.Add("*");
        StorageFile? file = await picker.PickSingleFileAsync();
        if (file is null) return null;

        IRandomAccessStreamWithContentType stream = await file.OpenReadAsync();
        var managed = stream.AsStreamForRead();
        var name = Path.GetFileNameWithoutExtension(file.Name);
        var ext = Path.GetExtension(file.Name).TrimStart('.');
        return new PickedFile
        {
            Stream = managed,
            FileName = name,
            Extension = ext
        };
    }

    private static FileOpenPicker CreatePicker()
    {
#if WINDOWS
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current?.Window as Microsoft.UI.Xaml.Window);
        var picker = new FileOpenPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        return picker;
#else
        return new FileOpenPicker();
#endif
    }
}
