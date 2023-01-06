using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using DynamicData;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace srra;

public partial class MainWindow : Window
{
    readonly MainWindowViewModel _mainWindowViewModel;
    public MainWindow()
    {
        InitializeComponent();
        _mainWindowViewModel = new MainWindowViewModel();
        DataContext = _mainWindowViewModel;
        SetEventHandlers();
        ProcessData();
    }

    public async void ProcessData()
    {
        var matches = await MatchLoader.LoadMatches();
        var replayReader = new ReplayReader(this, _mainWindowViewModel, matches);
        await replayReader.ReadReplays();
    }

    public void ShowGraphData(List<Match> replayData)
    {
        throw new NotImplementedException();
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
        if (MatchesDataGrid.SelectedItem is Match match) 
            match.OpenReplayFolder();
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
