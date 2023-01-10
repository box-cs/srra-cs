using Avalonia.Controls;
using Avalonia.Interactivity;
using srra.ViewModels;
using System.Configuration;
using System;
using DynamicData;
using srra.Starcraft;
using srra.Analyzers;
using System.Collections.Generic;
using System.Linq;

namespace srra;

public partial class MainWindow : Window
{
    readonly MainWindowViewModel _mainWindowViewModel;
    readonly Analyzer _analyzer;
    readonly ReplayReader replayReader = new();
    public List<string> PlayerNames { get => ConfigurationManager.AppSettings["PlayerNames"]?.Split(',').ToList() ?? new(); }
    public MainWindow()
    {
        InitializeComponent();
        UpdateStatisticsTabVisibility(false);
        _mainWindowViewModel = new MainWindowViewModel();
        _analyzer = new Analyzer(this, _mainWindowViewModel);
        DataContext = _mainWindowViewModel;
        if (PlayerNames.Any(name => string.IsNullOrEmpty(name)))
            _mainWindowViewModel.IsPlayerNameSet = false;
        SetEventHandlers();
        ProcessData();
    }

    public async void ProcessData()
    {
        _analyzer.IsDoneAnalyzing = false;
        StatusLabel.Content = "Loading ...";
        replayReader.replayData.Clear();
        _mainWindowViewModel.Matches.Clear();
        _mainWindowViewModel.SimpleWinRates.Clear();

        replayReader.SetReplayPaths();
        await replayReader.ReadReplaysTask();
        _analyzer.AnalyzeReplays(replayReader.replayData);

        _mainWindowViewModel.Matches.AddRange(replayReader.replayData);
        _mainWindowViewModel.SimpleWinRates.AddRange(_analyzer.WinRates.ToSimpleWinRates());
        _analyzer.UpdateGraphData();
        _analyzer.IsDoneAnalyzing = true;
        StatusLabel.Content = $"Found {replayReader.replayData.Count} replays!";
    }

    private void SetEventHandlers()
    {
        MatchesDataGrid.DoubleTapped += MatchesDataGrid_DoubleTapped;
        ViewReplayDetailsMenuItem.Click += ViewReplayDetailsMenuItem_Click;
        OpenFolderLocationMenuItem.Click += OpenFolderLocationMenuItem_Click;
        DeleteFileMenuItem.Click += DeleteFileMenuItem_Click;
        OptionsMenuItem.Click += OptionsMenuItem_Click;
        StatisticsMenuItem.Click += StatisticsMenuItem_Click;
        ExitMenuItem.Click += ExitMenuItem_Click;
        TableMenuItem.Click += TableMenuItem_Click;
    }

    private void MatchesDataGrid_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (MatchesDataGrid.SelectedItem is not Match match) return;
        throw new NotImplementedException();
    }

    private void ViewReplayDetailsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void DeleteFileMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        List<Match> itemsToDelete = new();
        foreach (var item in MatchesDataGrid.SelectedItems)
            if (item is Match match) 
                itemsToDelete.Add(match);   

        itemsToDelete.ForEach(match => {
            match.DeleteReplayFile(out var success);
            if (!success) return;
            replayReader.replayData.Remove(match);
            _mainWindowViewModel.Matches.Remove(match);
        });
    }

    private void OpenFolderLocationMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var item in MatchesDataGrid.SelectedItems) 
            if (item is Match match) 
                match.OpenReplayFolder();
    }

    private async void OptionsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var path = ReplayReader.ScrepPath;
        var playerNames = PlayerNames;
        var replayPath = ReplayReader.ReplayPath;

        var optionsDialog = new OptionsDialog();
        var result = optionsDialog.ShowDialog(this);
        await result;

        // If we've made any changes, re-process the data
        if (ReplayReader.ScrepPath != path ||
            !playerNames.SequenceEqual(PlayerNames) ||
            replayPath != ReplayReader.ReplayPath)
            ProcessData();
    }

    private void StatisticsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (_mainWindowViewModel.IsPlayerNameSet ||
            PlayerNames.Any(name => !string.IsNullOrEmpty(name)) &&
             _analyzer.IsDoneAnalyzing) {
            UpdateTableViewTabVisibility(false);
            UpdateStatisticsTabVisibility(true);
            return;
        }
        var messageBox = new MessageBox(_analyzer.IsDoneAnalyzing ? "Set a player name!" : "Wait for analyzer!", "Ok");
        messageBox.ShowDialog(this);
    }
    
    private void ExitMenuItem_Click(object? sender, RoutedEventArgs e) => Close();

    private void TableMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        UpdateTableViewTabVisibility(true);
        UpdateStatisticsTabVisibility(false);
    }

    private void UpdateStatisticsTabVisibility(bool isVisible)
    {
        StatisticsGrid.IsVisible = isVisible;
        StatisticsPlot.IsVisible = isVisible;
    }

    private void UpdateTableViewTabVisibility(bool isVisible)
    {
        MatchesDataGrid.IsVisible = isVisible;
        StatusLabel.IsVisible = isVisible;
    }
}

