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

    public async void ChunkilyAnalyzeReplays(Action<List<Match>> action, Action closeAction)
    {
        var chunkedReplayPaths = ReplayPaths?.Chunk(35);
        if (chunkedReplayPaths != null)
        {
            foreach (var chunk in chunkedReplayPaths)
            {
                var chunkedMatches = await Task.Run(() =>
                {
                    List<Match> chunkedMatches = new();
                    foreach (var replayPath in chunk)
                    {
                        if (string.IsNullOrEmpty(_screpPath) || ReplayPaths?.Count == 0)
                            continue;

                        var match = ReadReplay(replayPath);
                        if (match is not null)
                            chunkedMatches.Add(match);
                    }
                    return chunkedMatches;
                });
                action.Invoke(chunkedMatches);
            }
            closeAction.Invoke();
        }
    }

    public Match? ReadReplay(string replayPath)
    {
        var data = ReadFromSCREP(replayPath);
        if (string.IsNullOrEmpty(data)) return null;
        return new Match(data, replayPath);
    }

    public List<Task> GenerateGetMatchesTask()
    {
        List<Match> matches = new();
        if (ReplayPaths == null) return new();

        var chunks = ReplayPaths.Chunk(50);
        var tasks = new List<Task> {
            new Task(() => {
                foreach (var chunk in chunks)
                    foreach (var replayPath in chunk) {
                        var match = ReadReplay(replayPath);
                        if (match != null) {
                            matches.Add(match);
                        }
                    }
            })
        };
        return tasks;
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