using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;
using DynamicData;
using System.Collections.Generic;

namespace srra;


public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var mainWindowViewModel = new MainWindowViewModel();
        DataContext = mainWindowViewModel;
        var matchLoader = new MatchLoader(this, mainWindowViewModel);
        matchLoader.LoadMatches();
        SetEventHandlers();
        // Find a way to resize the entire window dynamically
        MatchesDataGrid.Height = SRRAWindow.Height - 80;
        ShowGraphData(mainWindowViewModel);
    }

    private void ShowGraphData(MainWindowViewModel mainWindowVM)
    {
        var graph = new Graph(StatisticsPlot, "APM Graph") {
            xData = new double[] { 1, 2, 3 },
            yData = new double[] { 1, 2, 3 }
        };
        graph.ShowGraph();
        mainWindowVM.RefreshDataGrid(new() {
            new Match() {
                Name = "Fox",
                APM = "305/205",
                OpponentName = "jinjin5000",
                OpponentApm = "278, 203",
                MatchUp = "TvT",
                Map = "Polypoid",
                Result = "L",
                Date = DateTime.Now.ToShortDateString()
            },
            new Match() {
                Name = "Flash",
                APM = "423/305",
                OpponentName = "Jaedong",
                OpponentApm = "410, 296",
                MatchUp = "TvZ",
                Map = "Eclipse",
                Result = "L",
                Date = DateTime.Now.ToShortDateString()
            }
        });
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
