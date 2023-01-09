using System.Collections.Generic;
using System.Linq;
using srra.ViewModels;
using srra.Starcraft;
using System.Data;

namespace srra.Analyzers;

public class Analyzer
{
    readonly MainWindow _mainWindow;
    readonly MainWindowViewModel _mainWindowViewModel;
    public WinRates WinRates = new();

    public bool IsDoneAnalyzing { get; set; }

    public Analyzer(MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
    {
        _mainWindow = mainWindow;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void UpdateGraphData()
    {
        var apmResults = GetAPMResults();
        var xData = Enumerable.Range(0, apmResults.Count).Select(x => (double)x).ToArray();
        var graph = new Graph(_mainWindow.StatisticsPlot, "APM Graph", "Matches", "APM")
        {
            xData = xData,
            yData = apmResults.Select(x => (double?)x ?? 0.0).ToArray()
        };
        graph.ShowGraph();
    }

    public void AnalyzeReplays(List<Match> matches)
    {
        SetWinRatios(matches);
    }

    public void SetWinRatios(List<Match> matches)
    {
        if (_mainWindow.PlayerNames.Any(name => string.IsNullOrEmpty(name))) return;

        WinRates = new WinRates();
        matches.ForEach(match =>
        {
            var player = match.Players.Find(p => p?.Name != null && _mainWindow.PlayerNames.Contains(p.Name));
            if (player != null) 
            {
                var opponent = match.Players.Find(p => p.ID != player.ID)!;
                WinRates[player.Race!][opponent.Race!]["Games"]++;
                if (match.WinnerTeam == player.TeamID)
                    WinRates[player.Race!][opponent.Race!]["Wins"]++;
            }
        });
    }

    private List<int?> GetAPMResults()
    {
        var apmResults = new List<int?>();

        if (_mainWindow.PlayerNames.Any(name=>string.IsNullOrEmpty(name))) return apmResults;

        foreach (var match in _mainWindowViewModel.Matches)
        {
            var player = match?.Players.Find(p => p?.Name != null && _mainWindow.PlayerNames.Contains(p.Name));
            if (player is null) continue;
            apmResults.Add(player.APM);
        }
        return apmResults;
    }
}
