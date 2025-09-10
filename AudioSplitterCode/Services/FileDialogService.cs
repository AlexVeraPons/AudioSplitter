using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace AudioSplitter.Services;

public interface IFileDialogService
{
    IEnumerable<string> ShowOpenAudioFiles();
    string? BrowseFolder(string description);
}

public sealed class FileDialogService : IFileDialogService
{
    public IEnumerable<string> ShowOpenAudioFiles()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select audio files",
            Filter = "Audio files|*.mp3;*.wav;*.m4a;*.aac;*.flac;*.ogg|All files|*.*",
            Multiselect = true
        };
        return dlg.ShowDialog() == true ? dlg.FileNames : Enumerable.Empty<string>();
    }

    public string? BrowseFolder(string description)
    {
        var dlg = new VistaFolderBrowserDialog { Description = description, UseDescriptionForTitle = true };
        return dlg.ShowDialog() == true ? dlg.SelectedPath : null;
    }
}
