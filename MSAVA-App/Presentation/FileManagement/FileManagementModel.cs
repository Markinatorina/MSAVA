namespace MSAVA_App.Presentation.FileManagement;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MSAVA_App.Services.Files;
using MSAVA_Shared.Models;
using Uno.Extensions;

public partial record FileManagementModel : INotifyPropertyChanged
{
    private readonly IDispatcher _dispatcher;
    private readonly FileRetrievalService _filesService;

    public FileManagementModel(IDispatcher dispatcher, FileRetrievalService filesService)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _filesService = filesService ?? throw new ArgumentNullException(nameof(filesService));
        Files = new ObservableCollection<SearchFileDataDTO>();

        _ = LoadAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value) return;
            _isLoading = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoaded)));
        }
    }

    public bool IsLoaded => !IsLoading;

    public ObservableCollection<SearchFileDataDTO> Files { get; }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (IsLoading) return;

        await _dispatcher.ExecuteAsync(() => IsLoading = true);

        try
        {
            var data = await _filesService.GetAllFileMetadataAsync(ct);

            await _dispatcher.ExecuteAsync(() =>
            {
                Files.Clear();
                foreach (var item in data)
                {
                    Files.Add(item);
                }
            });
        }
        finally
        {
            await _dispatcher.ExecuteAsync(() => IsLoading = false);
        }
    }

    // Used by Button Command binding
    public Task Refresh() => LoadAsync();
}
