using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace srra
{
    public enum GameType
    {
        None,
        Custom,
        Melee,
        FreeForAll,
        OneOnOne,
        CaptureTheFlag,
        Greed,
        Slaughter,
        SuddenDeath,
        Ladder,
        UMS,
        TeamMelee,
        TeamFFA,
        TeamCTF,
        Unkown,
        TopVsBottom,
    };

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
            replayPaths.Reverse(); // Order by latest date
            var replayReader = new ReplayReader(_mainWindow, _mainWindowVM, screpPath, replayPaths);
            System.Diagnostics.Trace.WriteLine($"Found {replayPaths.Count} Replays!");
            await replayReader.ReadReplays();
            System.Diagnostics.Trace.WriteLine($"Replay Paths: {replayPaths.Count}, Analyzed Replays: {_mainWindowVM.Matches.Count}");
            _mainWindow.ShowGraphData();
        }
    }
}
