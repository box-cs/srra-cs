using System.Collections.Generic;
using System.Configuration;
using System.Linq;
// Analyzes matches to calculate match up win rates and get apm/eapm graph
namespace srra
{
    public class Analyzer
    {
        MainWindow _mainWindow;
        MainWindowViewModel _mainWindowViewModel;

        public Analyzer(MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void ShowGraphData()
        {
            var apmResults = GetAPMResults();
            var xData = Enumerable.Range(0, apmResults.Count).Select(x => (double)x).ToArray();
            var graph = new Graph(_mainWindow.StatisticsPlot, "APM Graph") {
                xData = xData,
                yData = apmResults.Select(x => (double?)x ?? 0.0).ToArray()
            };
            graph.ShowGraph();
        }

        private List<int?> GetAPMResults()
        {
            var apmResults = new List<int?>();
            var playerName = ConfigurationManager.AppSettings["PlayerName"];
            foreach (var match in _mainWindowViewModel.Matches) {
                var player = match?.Players?.Find(player => player.Name == playerName);
                if (player is null) continue;
                apmResults.Add(player.APM);
            }
            return apmResults;
        }
    }
}
