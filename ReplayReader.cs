using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.Linq;

namespace srra;

public class ReplayReader
{
    public List<Match> replayData = new();
    public List<Thread> processingThreads = new();
    readonly MainWindow _mainWindow;
    readonly MainWindowViewModel _mainWindowVM;
    private readonly List<string>? _replayPaths;
    private int _count;

    public ReplayReader(MainWindow mainWindow, MainWindowViewModel mainWindowVM, List<string> replayPaths)
    {
        _mainWindow = mainWindow;
        _mainWindowVM = mainWindowVM;   
        _replayPaths = replayPaths;
        _count = 0;
    }

    public Match? ReadReplay(string replayPath, string? _screpPath)
    {
        if (string.IsNullOrEmpty(_screpPath) || _replayPaths?.Count == 0) return null;
        var data = ReadFromSCREP(_screpPath, replayPath);

        if (string.IsNullOrEmpty(data)) return null;
    
        return new Match(data, replayPath);
    }

    public async Task<List<Match>> ReadReplays()
    {
        if (_replayPaths == null) return new();
        string? screpPath = ConfigurationManager.AppSettings["SCREP_Path"];

        var chunks = _replayPaths.Chunk(50);
        var tasks = new List<Task>();
        var matches = new List<Match>();
        foreach (var chunk in chunks)
            tasks.Add(new Task(() => {
                foreach (var replayPath in chunk) {
                    var match = ReadReplay(replayPath, screpPath);
                    if (match != null)
                        matches.Add(match);
                }
            }));
        tasks.ForEach(task=>task.Start());
        await Task.WhenAll(tasks);
        return matches;
    }

    private static string? ReadFromSCREP(string screpPath, string replayPath)
    {
        using var proc = new Process() {
            StartInfo = new ProcessStartInfo() {
                UseShellExecute = false,
                FileName = $"{screpPath}\\screp.exe",
                CreateNoWindow = true,
                Arguments = $"-map \"{replayPath}\"",
                RedirectStandardOutput = true,
            },
        };
        proc.Start();
        var data = proc.StandardOutput.ReadToEnd();
        return proc.ExitCode == 0 ? data : null;
    }

    private void UpdateProgressBar()
    {
        if (_replayPaths == null) return;

        if (_count == 0) 
            (new Action(() => _mainWindow.SRRAProgressBar.IsVisible = true)).Invoke();
        if (_count == _replayPaths.Count - 1)
            (new Action(() => _mainWindow.SRRAProgressBar.IsVisible = false)).Invoke();

        new Action(() => _mainWindow.SRRAProgressBar.Value = (++_count * 100) / _replayPaths.Count).Invoke();
    }
}