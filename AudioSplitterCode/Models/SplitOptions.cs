namespace AudioSplitter.Models;

public sealed class SplitOptions
{
    public int ChunkMinutes { get; init; } = 5;
    public int StartOffsetSeconds { get; init; }
    public bool UseSourceFolder { get; init; }
    public string OutputFolder { get; init; } = "";
    public string FilePattern { get; init; } = "{series:D2}:{part:D2} {base}.mp3";
    public bool ReplaceTitle { get; init; }
}
