using System.IO;
using AudioSplitter.Infrastructure;
using AudioSplitter.Models;

namespace AudioSplitter.Services;

public interface IAudioSplitterService
{
    Task SplitAsync(string inputPath, SplitOptions options, IProgress<double> perFileProgress, CancellationToken ct);
}

public sealed class AudioSplitterService : IAudioSplitterService
{
    private readonly IFilePatternService _pattern;
    private readonly ITagWriter _tagWriter;
    private readonly IProcessRunner _proc;
    private readonly string _ffmpegExe;

    public AudioSplitterService(IFilePatternService pattern, ITagWriter tagWriter, IProcessRunner proc, string ffmpegExe)
    {
        _pattern = pattern; _tagWriter = tagWriter; _proc = proc; _ffmpegExe = ffmpegExe;
    }

    public async Task SplitAsync(string inputPath, SplitOptions opt, IProgress<double> perFileProgress, CancellationToken ct)
    {
        string outFolder = opt.UseSourceFolder ? Path.GetDirectoryName(inputPath)! : opt.OutputFolder;
        Directory.CreateDirectory(outFolder);

        var (ffOutPattern, _) = _pattern.BuildPattern(inputPath, outFolder, fileIndex: 1, opt.FilePattern);

        int seconds = Math.Max(1, opt.ChunkMinutes * 60);
        int start = Math.Max(0, opt.StartOffsetSeconds);

        string common = $"-hide_banner -loglevel error -y {(start > 0 ? $"-ss {start} " : "")}-i \"{inputPath}\" -f segment -segment_time {seconds} -segment_start_number 1 -reset_timestamps 1 -map 0:a:0";
        string copyArgs = $"{common} -c copy \"{ffOutPattern}\"";

        var code = await _proc.RunAsync(_ffmpegExe, copyArgs, ct);

        if (code != 0)
        {
            string encArgs = $"{common} -c:a libmp3lame -b:a 192k \"{ffOutPattern}\"";
            code = await _proc.RunAsync(_ffmpegExe, encArgs, ct);
            if (code != 0) throw new IOException("ffmpeg failed to split the file.");
        }

        await _tagWriter.RetagAsync(outFolder, ffOutPattern, opt.ReplaceTitle, _ffmpegExe, _proc, ct);
        perFileProgress.Report(100);
    }
}
