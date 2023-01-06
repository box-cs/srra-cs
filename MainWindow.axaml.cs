using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using DynamicData;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;

namespace srra;

public partial class MainWindow : Window
{
    MainWindowViewModel _mainWindowViewModel;
    public MainWindow()
    {
        InitializeComponent();
        _mainWindowViewModel = new MainWindowViewModel();
        DataContext = _mainWindowViewModel;
        SetEventHandlers();
        var matchLoader = new MatchLoader(this, _mainWindowViewModel);
        matchLoader.LoadMatches();
    }

    public void ShowGraphData()
    {
        return; // Bugged for now
        var xData = new double[] { 1, 2, 3 };
        var yData = new double[] { 1, 2, 3 };
        var playerName = ConfigurationManager.AppSettings["PlayerName"];
        List<double> apmResults = new List<double>();

        if (playerName is not null) {
            foreach (var match in _mainWindowViewModel.Matches) {
                var player = match.Players.Find(player => player.Name == playerName)!;
                if (double.TryParse(player.APM.ToString(), out var apmResult)){ 
                    apmResults.Add(apmResult);
                }
            }
            var data =  Enumerable.Range(0, apmResults.Count).Select(x => (double)x).ToList();
            xData = data.ToArray();
            yData = apmResults.ToArray();
        }

        var graph = new Graph(StatisticsPlot, "APM Graph!!") {
            xData = xData,
            yData = yData,
        };

        graph.ShowGraph();
    }

    private void SetEventHandlers()
    {
        MatchesDataGrid.DoubleTapped += MatchesDataGrid_DoubleTapped;
        TableMenuItem.Click += TableMenuItem_Click;
        StatisticsMenuItem.Click += StatisticsMenuItem_Click;
        ExitMenuItem.Click += ExitMenuItem_Click;
        OptionsMenuItem.Click += OptionsMenuItem_Click;
    }
    private async void OptionsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        using var optionsDialog = new OptionsDialog();
        await (optionsDialog.ShowDialog(this));
    }

    private void ExitMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    private void StatisticsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        DataGridScrollViewer.IsVisible = false;
        MatchesDataGrid.IsVisible = false;
        StatisticsGrid.IsVisible = true;
        StatisticsPlot.IsVisible = true;
    }

    private void TableMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        DataGridScrollViewer.IsVisible = true;
        MatchesDataGrid.IsVisible = true;
        StatisticsGrid.IsVisible = false;
        StatisticsPlot.IsVisible = false;
    }

    private void MatchesDataGrid_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        // Potentially we can see more statistics for the match ? 
        // throw new Exception(MatchesDataGrid.SelectedItem.ToString());
    }
}

public class MainWindowViewModel
{
    public ObservableCollection<Match> Matches { get; set; } = new();
    public void RefreshDataGrid(List<Match> matches)
    {
        Matches.Clear();
        Matches.AddRange(matches);
    }
}
