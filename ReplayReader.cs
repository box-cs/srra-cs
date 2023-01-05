using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace srra;

public class ReplayReader
{
    readonly MainWindow _mainWindow;
    public List<string> replayData = new();
    public List<Thread> processingThreads = new();
    private readonly string? _screpPath;
    private readonly List<string>? _replayPaths;
    private string _incomingData;


    public ReplayReader(MainWindow mainWindow, string screpPath, List<string> replayPaths)
    {
        _mainWindow = mainWindow;
        _screpPath = screpPath;
        _replayPaths = replayPaths;
    }

    public async Task ReadReplay(string replayPath)
    {
            if (_screpPath is null || _replayPaths?.Count == 0) return;
            try {
                
                using var proc = new Process() {
                    StartInfo = new ProcessStartInfo() {
                        UseShellExecute = false,
                        FileName = $"{_screpPath}\\screp.exe",
                        CreateNoWindow = true,
                        Arguments = $"-map {replayPath}",
                        RedirectStandardOutput = true
                    },
                };

            proc.Start();
            _incomingData = await proc.StandardOutput.ReadToEndAsync();
            // Success exit code from parser
            if (proc.ExitCode == 0) 
                (new Action(() => replayData.Add(_incomingData) )).Invoke();
            }
            catch (Exception) {
                // Ignored
            }
    }

    public async Task ReadReplays()
    {
        if (_replayPaths == null) return;
        var count = 0;
        (new Action(() => _mainWindow.SRRAProgressBar.IsVisible = true)).Invoke();
        foreach (var replayPath in _replayPaths) {
            await ReadReplay(replayPath);
            (new Action(() => _mainWindow.SRRAProgressBar.Value = (++count * 100) / _replayPaths.Count)).Invoke();
        }
        if (count == _replayPaths.Count) {
            (new Action(() => _mainWindow.SRRAProgressBar.IsVisible = false)).Invoke();
        }
    }
}