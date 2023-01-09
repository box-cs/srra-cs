using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.IO;
using System.Configuration;

namespace srra.Starcraft;

public class ReplayReader
{
    public List<Match> replayData = new();
    public List<Thread> processingThreads = new();
    public List<string> ReplayPaths = new();
    public static readonly string? ScrepPath = ConfigurationManager.AppSettings["SCREP_Path"];

    public ReplayReader() { }

    public async Task ReadReplaysTask()
    {
        replayData.Clear();
        await Task.Run(() => {
            const int MAX_NUMBER_OF_THREADS = 12;
            var paths = ReplayPaths;
            var chunkedPaths = paths.Chunk(paths.Count > MAX_NUMBER_OF_THREADS ? paths.Count / MAX_NUMBER_OF_THREADS : paths.Count);
            Parallel.For(0, chunkedPaths.Count(), (count, state) => {
                chunkedPaths.ToList()[count].ToList().ForEach(path => {
                    var match = ReadReplay(path);
                    if (match != null) {
                        replayData.Add(match);
                    }
                });
            });
        });
        replayData.Sort((a, b) => Nullable.Compare(b.Date, a.Date));
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
                FileName = $"{ScrepPath}\\screp.exe",
                CreateNoWindow = true,
                Arguments = $"-map -indent=false \"{replayPath}\"",
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

        if (string.IsNullOrEmpty(replayPath)) return;
        var matches = Directory.GetFiles(replayPath, "*.rep", SearchOption.AllDirectories).ToList();
        matches.Reverse();
        ReplayPaths = new List<string>(matches);
    }
}