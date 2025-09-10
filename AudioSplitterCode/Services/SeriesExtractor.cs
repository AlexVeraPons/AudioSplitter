using System.Text.RegularExpressions;

namespace AudioSplitter.Services;

public interface ISeriesExtractor
{
    int Extract(string baseName, int fallback);
}

public sealed class SeriesExtractor : ISeriesExtractor
{
    public int Extract(string name, int fallback)
    {
        var m = Regex.Match(name, @"(?ix)
            \b(?:book|bk|vol(?:ume)?|v|episode|ep|part|p)\s*(\d+)\b
            | (?<!\d)(\d{1,3})(?!\d)
            | \b(?:book|bk|vol|v|ep|episode|part|p)\s*([IVXLCDM]+)\b
        ");
        if (m.Success)
        {
            foreach (var g in m.Groups.Cast<Group>().Skip(1))
                if (g.Success && int.TryParse(g.Value, out int n)) return n;

            foreach (var g in m.Groups.Cast<Group>().Skip(1))
                if (g.Success && Regex.IsMatch(g.Value, "^[IVXLCDM]+$", RegexOptions.IgnoreCase))
                    return RomanToInt(g.Value);
        }
        return fallback;
    }

    private static int RomanToInt(string s)
    {
        var map = new Dictionary<char, int> { ['I'] = 1, ['V'] = 5, ['X'] = 10, ['L'] = 50, ['C'] = 100, ['D'] = 500, ['M'] = 1000 };
        int total = 0, prev = 0;
        foreach (char ch in s.ToUpper().Reverse())
        {
            int val = map[ch];
            total += val < prev ? -val : val;
            prev = val;
        }
        return total;
    }
}
