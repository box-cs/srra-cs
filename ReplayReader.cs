using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

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

    public async Task ReadReplay(string replayPath)
    {
        string? _screpPath = ConfigurationManager.AppSettings["SCREP_Path"];
        if (_screpPath is null || _replayPaths?.Count == 0) return;
        try {
            var data = await ReadFromSCREP(_screpPath, replayPath);
            if (data is null) return;
            var match = new Match(data, replayPath);
            if (match.IsLadderMatch) {
                _mainWindowVM.Matches.Add(match);
                replayData.Add(match);
            }
        }
        catch (Exception) {
            // Ignored
        }
    }

    public async Task ReadReplays()
    {
        if (_replayPaths == null) return;
        foreach (var replayPath in _replayPaths) {
            await ReadReplay(replayPath);
            UpdateProgressBar();
        }
    }

    private static async Task<string?> ReadFromSCREP(string screpPath, string replayPath)
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
        var data = await proc.StandardOutput.ReadToEndAsync();
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