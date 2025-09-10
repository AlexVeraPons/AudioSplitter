using System.IO;

namespace AudioSplitter.Models;
public sealed class Track
{
    public string FullPath { get; }
    public string DisplayName => Path.GetFileName(FullPath);
    public Track(string fullPath) => FullPath = fullPath;
}

