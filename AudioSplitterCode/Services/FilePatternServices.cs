using System.IO;
using System.Text.RegularExpressions;

namespace AudioSplitter.Services;

public interface IFilePatternService
{
    (string ffmpegPattern, string previewName) BuildPattern(string inputPath, string folder, int fileIndex, string filePattern);
}

public sealed class FilePatternService : IFilePatternService
{
    private readonly ISeriesExtractor _series;

    public FilePatternService(ISeriesExtractor series) => _series = series;

    public (string ffmpegPattern, string previewName) BuildPattern(string inputPath, string folder, int fileIndex, string pattern)
    {
        string baseName = Path.GetFileNameWithoutExtension(inputPath);
        int series = _series.Extract(baseName, fileIndex);

        pattern ??= "";

        pattern = pattern.Replace("{base}", baseName);

        pattern = Regex.Replace(pattern, @"\{series(?::D(?<d>\d+))?\}", m =>
        {
            if (m.Groups["d"].Success && int.TryParse(m.Groups["d"].Value, out int w)) return series.ToString($"D{w}");
            return series.ToString();
        });

        pattern = Regex.Replace(pattern, @"\{part(?::D(?<d>\d+))?\}", m =>
            m.Groups["d"].Success ? $"%0{m.Groups["d"].Value}d" : "%d");

        pattern = Path.GetFileName(string.IsNullOrWhiteSpace(pattern) ? "output_%03d.mp3" : pattern);
        foreach (var c in Path.GetInvalidFileNameChars()) pattern = pattern.Replace(c.ToString(), "_");

        var matches = Regex.Matches(pattern, @"%(\d+)?d");
        if (matches.Count == 0)
        {
            string nameNoExt = Path.GetFileNameWithoutExtension(pattern);
            string ext = Path.GetExtension(pattern);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".mp3";
            pattern = $"{nameNoExt}_%03d{ext}";
        }
        else if (matches.Count > 1)
        {
            throw new IOException("Your file pattern results in multiple {part} placeholders. Please use only one.");
        }

        return (Path.Combine(folder, pattern), pattern);
    }
}
