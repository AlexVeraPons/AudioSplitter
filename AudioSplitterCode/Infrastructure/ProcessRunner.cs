using System.Diagnostics;

namespace AudioSplitter.Infrastructure;

public interface IProcessRunner
{
    Task<int> RunAsync(string fileName, string arguments, CancellationToken ct = default);
    Task<(int code, string stdout, string stderr)> RunWithOutputAsync(string fileName, string arguments, CancellationToken ct = default);
}

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<int> RunAsync(string fileName, string arguments, CancellationToken ct = default)
    {
        var (code, _, _) = await RunWithOutputAsync(fileName, arguments, ct);
        return code;
    }

    public async Task<(int code, string stdout, string stderr)> RunWithOutputAsync(string fileName, string arguments, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
        p.Start();
        Task wait = Task.Run(async () => { await p.WaitForExitAsync(ct); }, ct);

        var stdOutTask = p.StandardOutput.ReadToEndAsync();
        var stdErrTask = p.StandardError.ReadToEndAsync();

        await Task.WhenAll(wait, stdOutTask, stdErrTask);
        return (p.ExitCode, await stdOutTask, await stdErrTask);
    }
}
