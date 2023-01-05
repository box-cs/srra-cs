using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace srra
{
    public class MatchLoader
    {
        readonly MainWindow _mainWindow;
        readonly MainWindowViewModel _mainWindowVM;
        public MatchLoader(MainWindow mainWindow, MainWindowViewModel mainWindowVM)
        {
            _mainWindow = mainWindow;
            _mainWindowVM = mainWindowVM;
        }

        public async void LoadMatches()
        {
            var replayPath = ConfigurationManager.AppSettings["Replay_Path"];
            var screpPath = ConfigurationManager.AppSettings["SCREP_Path"];

            if (screpPath is null || replayPath is null) return;
            var replayPaths = Directory.GetFiles(replayPath, "*.rep", SearchOption.AllDirectories).ToList();
            var replayReader = new ReplayReader(_mainWindow, screpPath, replayPaths);
            System.Diagnostics.Trace.WriteLine($"Found {replayPaths.Count} Replays!");
            await replayReader.ReadReplays();
            replayReader.replayData.ForEach(rep => {
                try {
                    var match = new Match(rep);
                    match.PrintMatch();
                    _mainWindowVM.Matches.Add(match);
                }
                catch (Exception e) {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                    // Ignore
                }
            });
            System.Diagnostics.Trace.WriteLine($"Replay Paths: {replayPaths.Count}, Analyzed Replays: {replayReader.replayData.Count}");
        }
    }
}
