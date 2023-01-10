using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Configuration;

namespace srra.Starcraft;

public class ReplayReader
{
    public List<Match> replayData = new();
    public List<string> ReplayPaths = new();
    public static string? ScrepPath { get => ConfigurationManager.AppSettings["SCREP_Path"]; }
    public static string? ReplayPath { get => ConfigurationManager.AppSettings["Replay_Path"]; }

    public ReplayReader() { }

    public async Task ReadReplaysTask()
    {
        // BUG: Occasionally it misses a file or two
        replayData.Clear();
        await Task.Run(() => {
            const int MAX_NUMBER_OF_THREADS = 12;
            var paths = ReplayPaths;
            if (paths.Count() == 0) return;
            var chunkSize = paths.Count > MAX_NUMBER_OF_THREADS ? (paths.Count / MAX_NUMBER_OF_THREADS) : paths.Count;
            var chunkedPaths = paths.Chunk(chunkSize);
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
        using var proc = new Process() {
            StartInfo = new ProcessStartInfo() {
                UseShellExecute = false,
                FileName = $"{ScrepPath}\\screp.exe",
                CreateNoWindow = true,
                Arguments = $"-map -indent=false \"{replayPath}\"",
                RedirectStandardOutput = true,
            },
        };
        proc.Start();
        var data = proc.StandardOutput.ReadToEnd();
        return (proc.ExitCode == 0) ? data : null;
    }

    public void SetReplayPaths()
    {
        if (string.IsNullOrEmpty(ReplayPath)) return;

        var matches = Directory.EnumerateFiles(ReplayPath, "*.rep", SearchOption.AllDirectories)
            .Where(path=>path.EndsWith(".rep"));
        ReplayPaths = new List<string>(matches.Reverse());
    }
}