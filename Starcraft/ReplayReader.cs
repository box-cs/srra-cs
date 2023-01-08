using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.Linq;
using System.IO;

namespace srra.Starcraft;

public class ReplayReader
{
    public List<Match> replayData = new();
    public List<Thread> processingThreads = new();
    public List<string> ReplayPaths = new();
    private readonly string? _screpPath;

    public ReplayReader(string? screpPath)
    {
        _screpPath = screpPath;
    }

    public Match? ReadReplay(string replayPath)
    {
        var data = ReadFromSCREP(replayPath);
        if (string.IsNullOrEmpty(data)) return null;
        return new Match(data, replayPath);
    }

    private string? ReadFromSCREP(string replayPath)
    {
        using var proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                FileName = $"{_screpPath}\\screp.exe",
                CreateNoWindow = true,
                Arguments = $"-map \"{replayPath}\"",
                RedirectStandardOutput = true,
            },
        };
        proc.Start();
        var data = proc.StandardOutput.ReadToEnd();
        return proc.ExitCode == 0 ? data : null;
    }

    public void SetReplayPaths()
    {
        var replayPath = ConfigurationManager.AppSettings["Replay_Path"];

        if (string.IsNullOrEmpty(_screpPath) || string.IsNullOrEmpty(replayPath)) return;
        var matches = Directory.GetFiles(replayPath, "*.rep", SearchOption.AllDirectories).ToList();
        matches.Reverse();
        ReplayPaths = new List<string>(matches);
    }
}