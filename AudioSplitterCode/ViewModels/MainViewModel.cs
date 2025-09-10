// ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AudioSplitter.Infrastructure;
using AudioSplitter.Models;
using AudioSplitter.Services;

namespace AudioSplitter.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    // ffmpeg.exe path once here
    private static readonly string FfmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg", "win-x64", "ffmpeg.exe");

    // UI state
    private int _chunkMinutes = 5;
    public int ChunkMinutes { get => _chunkMinutes; set { _chunkMinutes = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } }

    private int _startOffsetSeconds;
    public int StartOffsetSeconds { get => _startOffsetSeconds; set { _startOffsetSeconds = value; OnPropertyChanged(); } }

    private string _outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    public string OutputFolder { get => _outputFolder; set { _outputFolder = value; OnPropertyChanged(); } }

    private bool _useSourceFolder;
    public bool UseSourceFolder { get => _useSourceFolder; set { _useSourceFolder = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCustomOutputEnabled)); CommandManager.InvalidateRequerySuggested(); } }
    public bool IsCustomOutputEnabled => !UseSourceFolder;

    private bool _replaceTitle;
    public bool ReplaceTitle { get => _replaceTitle; set { _replaceTitle = value; OnPropertyChanged(); } }

    private string _filePattern = "{series:D2}:{part:D2} {base}.mp3";
    public string FilePattern { get => _filePattern; set { _filePattern = value; OnPropertyChanged(); } }

    private string _status = "";
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

    private double _progress;
    public double Progress { get => _progress; set { _progress = value; OnPropertyChanged(); } }

    public ObservableCollection<Track> TracksCollection { get; } = new();

    // Commands
    public RelayCommand AddFilesCommand { get; }
    public RelayCommand ClearFilesCommand { get; }
    public RelayCommand BrowseOutputCommand { get; }
    public RelayCommand SplitCommand { get; }

    // Services
    private readonly IFileDialogService _dialogs;
    private readonly IAudioSplitterService _splitter;

    private CancellationTokenSource? _cts;

    public MainViewModel()
        : this(new FileDialogService(),
               new AudioSplitterService(
                    new FilePatternService(new SeriesExtractor()),
                    new TagWriter(),
                    new ProcessRunner(),
                    FfmpegPath))
    { }

    public MainViewModel(IFileDialogService dialogs, IAudioSplitterService splitter)
    {
        _dialogs = dialogs; _splitter = splitter;

        AddFilesCommand = new RelayCommand(_ => OnAddFiles());
        ClearFilesCommand = new RelayCommand(_ => TracksCollection.Clear(), _ => TracksCollection.Any());
        BrowseOutputCommand = new RelayCommand(_ => OnBrowseOutput());

        SplitCommand = new RelayCommand(async _ => await SplitAllAsync(),
            _ => File.Exists(FfmpegPath)
                 && TracksCollection.Any()
                 && (UseSourceFolder || Directory.Exists(OutputFolder))
                 && ChunkMinutes > 0);

        TracksCollection.CollectionChanged += (_, __) => CommandManager.InvalidateRequerySuggested();
    }

    private void OnBrowseOutput()
    {
        var path = _dialogs.BrowseFolder("Select output folder");
        if (!string.IsNullOrWhiteSpace(path)) OutputFolder = path;
    }

    private void OnAddFiles()
    {
        foreach (var p in _dialogs.ShowOpenAudioFiles()
            .Where(File.Exists)
            .Where(p => !TracksCollection.Any(t => t.FullPath.Equals(p, StringComparison.OrdinalIgnoreCase))))
        {
            TracksCollection.Add(new Track(p));
        }
    }

    private async Task SplitAllAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            int total = TracksCollection.Count;
            int idx = 0;

            foreach (var t in TracksCollection)
            {
                ct.ThrowIfCancellationRequested();
                idx++;
                Status = $"[{idx}/{total}] {t.DisplayName}";
                var opt = new SplitOptions
                {
                    ChunkMinutes = ChunkMinutes,
                    StartOffsetSeconds = StartOffsetSeconds,
                    UseSourceFolder = UseSourceFolder,
                    OutputFolder = OutputFolder,
                    FilePattern = FilePattern,
                    ReplaceTitle = ReplaceTitle
                };

                var perFile = new Progress<double>(p => {});
                await _splitter.SplitAsync(t.FullPath, opt, perFile, ct);

                Progress = (double)idx / total * 100.0;
            }
            Status = "Done.";
        }
        catch (OperationCanceledException)
        {
            Status = "Canceled.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
