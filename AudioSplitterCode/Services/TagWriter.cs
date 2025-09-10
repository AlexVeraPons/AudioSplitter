using System.IO;
using System.Text.RegularExpressions;

namespace AudioSplitter.Services;

public interface ITagWriter
{
    Task RetagAsync(string outFolder, string ffmpegFileNamePattern, bool enabled, string ffmpegExe, Infrastructure.IProcessRunner runner, CancellationToken ct);
}

public sealed class TagWriter : ITagWriter
{
    private static string MakeTempMp3Path(string file)
    {
        var dir = Path.GetDirectoryName(file)!;
        var name = Path.GetFileNameWithoutExtension(file);
        var ext = Path.GetExtension(file);
        return Path.Combine(dir, $"{name}.tmp{ext}");
    }

    private static string Esc(string s) => s.Replace("\"", "\\\"");

    public async Task RetagAsync(string outFolder, string ffmpegPattern, bool enabled, string ffmpegExe, Infrastructure.IProcessRunner runner, CancellationToken ct)
    {
        if (!enabled) return;

        string filenameTpl = Path.GetFileName(ffmpegPattern);
        string rxPattern = "^" + Regex.Escape(filenameTpl)
                                .Replace("%03d", "(\\d+)")
                                .Replace("%02d", "(\\d+)")
                                .Replace("%d", "(\\d+)") + "$";
        var rx = new Regex(rxPattern, RegexOptions.IgnoreCase);

        var files = Directory.EnumerateFiles(outFolder, "*.mp3")
            .Where(p => rx.IsMatch(Path.GetFileName(p)))
            .OrderBy(p => int.Parse(rx.Match(Path.GetFileName(p)).Groups[1].Value))
            .ToList();

        foreach (var file in files)
        {
            string title = Path.GetFileNameWithoutExtension(file);
            string tmp = MakeTempMp3Path(file);
            string args = $"-hide_banner -loglevel error -y -i \"{file}\" -c copy -id3v2_version 3 -metadata title=\"{Esc(title)}\" \"{tmp}\"";
            var code = await runner.RunAsync(ffmpegExe, args, ct);
            if (code != 0) throw new IOException($"ffmpeg failed while tagging: {file}");
            File.Delete(file);
            File.Move(tmp, file);
        }
    }
}
