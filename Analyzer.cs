using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace srra;

public class Analyzer
{
    readonly MainWindow _mainWindow;
    readonly MainWindowViewModel _mainWindowViewModel;
    public WinRates? WinRates;

    public Analyzer(MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
    {
        _mainWindow = mainWindow;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void UpdateGraphData()
    {
        var apmResults = GetAPMResults();
        var xData = Enumerable.Range(0, apmResults.Count).Select(x => (double)x).ToArray();
        var graph = new Graph(_mainWindow.StatisticsPlot, "APM Graph") {
            xData = xData,
            yData = apmResults.Select(x => (double?)x ?? 0.0).ToArray()
        };
        graph.ShowGraph();
    }

    public void AnalyzeReplays(List<Match> matches)
    {
        FindWinRatios(matches);
        UpdateWinRatios();
    }

    public void FindWinRatios(List<Match> matches)
    {
        var playerName = ConfigurationManager.AppSettings["PlayerName"];
        if (string.IsNullOrEmpty(playerName)) return;

        WinRates = new WinRates();
        matches.ForEach(match => {
            var player = match.Players.Find(p => p.Name == playerName);
            if (player != null) {
                var opponent = match.Players.Find(p => p.ID != player.ID)!;
                WinRates[player.Race!][opponent.Race!]["Games"]++;
                if (match.WinnerTeam == player.TeamID)
                    WinRates[player.Race!][opponent.Race!]["Wins"]++;
            }
        });
    }

    public void UpdateWinRatios()
    {
        var races = new List<string>() { "Terran", "Zerg", "Protoss" };

        foreach (var player in races.Select((race, index) => (race, index))) 
            foreach (var opponent in races.Select((race, index) => (race, index))) {
                var textBlock = new TextBlock() {
                    Text = $"{Match.GetRaceAlias(player.race)}v{Match.GetRaceAlias(opponent.race)}: {WinRates[player.race][opponent.race].GetWinRate():P}",
                    [Grid.RowProperty] = player.index + 1,
                    [Grid.ColumnProperty] = opponent.index + 1,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                _mainWindow.WinRatesGrid.Children.Add(textBlock);
            }
    }

    private List<int?> GetAPMResults()
    {
        var apmResults = new List<int?>();
        var playerName = ConfigurationManager.AppSettings["PlayerName"];
        if (string.IsNullOrEmpty(playerName)) return apmResults;

        foreach (var match in _mainWindowViewModel.Matches) {
            var player = match?.Players?.Find(player => player.Name == playerName);
            if (player is null) continue;
            apmResults.Add(player.APM);
        }
        return apmResults;
    }
}
