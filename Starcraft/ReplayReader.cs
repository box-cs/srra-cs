using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Configuration;
using System.Collections.Concurrent;

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
        if (string.IsNullOrEmpty(ScrepPath) || !File.Exists($"{ScrepPath}\\screp.exe")) return;
        var paths = ReplayPaths;
        if (paths.Count == 0) return;
        var matches = await Task.Run(() => {
            var data = new ConcurrentQueue<Match>();
            var parallelOptions = new ParallelOptions {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0))
            };

            Parallel.ForEach(paths, parallelOptions, path => {
                data.Enqueue(ReadReplay(path));
            });
            return data.ToList();
        });
        replayData = new(matches);
        replayData.Sort((a, b) => DateTime.Compare(b.Date, a.Date));
    }

    public Match ReadReplay(string replayPath)
    {
        var data = ReadFromSCREP(replayPath);
        return (new ReplayLoader(data, replayPath)).ToMatch();
    }

    private string ReadFromSCREP(string replayPath)
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
        return (proc.ExitCode == 0) ? data : "";
    }

    public void SetReplayPaths()
    {
        if (string.IsNullOrEmpty(ReplayPath)) return;

        var matches = Directory.EnumerateFiles(ReplayPath, "*.rep", SearchOption.AllDirectories)
            .Where(path=>path.EndsWith(".rep"));
        ReplayPaths = new List<string>(matches.Reverse());
    }
}