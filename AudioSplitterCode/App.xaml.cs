using System.IO;
using System.Windows;
using AudioSplitter.Infrastructure;

namespace AudioSplitter;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!File.Exists(Paths.FfmpegExePath))
        {
            MessageBox.Show(
                "ffmpeg.exe was not found.\n\n" +
                "Expected at:\n" + Paths.FfmpegExePath +
                "\n\nPlease reinstall or restore the file.",
                "Missing FFmpeg",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Shutdown();
            return;
        }
    }
}

