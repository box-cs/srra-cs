using Avalonia.Controls;
using Avalonia.Interactivity;
using srra.ViewModels;
using System.Collections.Generic;
using System.Configuration;
using System;
using DynamicData;

namespace srra;

public partial class MainWindow : Window
{
    readonly MainWindowViewModel _mainWindowViewModel;
    readonly Analyzer _analyzer;

    public MainWindow()
    {
        InitializeComponent();
        UpdateStatisticsTabVisibility(false);
        _mainWindowViewModel = new MainWindowViewModel();
        _analyzer = new Analyzer(this, _mainWindowViewModel);
        DataContext = _mainWindowViewModel;
        var playerName = ConfigurationManager.AppSettings["PlayerName"];
        if (string.IsNullOrEmpty(playerName))
            _mainWindowViewModel.IsPlayerNameSet = false;
        SetEventHandlers();
        ProcessData();
    }


    public async void ProcessData()
    {
        string? screpPath = ConfigurationManager.AppSettings["SCREP_Path"];
        var replayPaths = await ReplayReader.LoadMatchesTask();
        List<Match> matches = new();
        var replayReader = new ReplayReader(replayPaths, screpPath);
        var fillDataGridAction = new Action<List<Match>>((matches) => _mainWindowViewModel.Matches.AddRange(matches));
        var closeAction = new Action(() => Close());
        replayReader.ChunkilyAnalyzeReplays(fillDataGridAction, closeAction);
        _analyzer.UpdateGraphData();
        _analyzer.AnalyzeReplays(new List<Match>(matches));
        _analyzer.IsDoneAnalyzing = true;
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
        throw new NotImplementedException();
    }

    private void ViewReplayDetailsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void DeleteFileMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OpenFolderLocationMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (MatchesDataGrid.SelectedItem is Match match)
            match.OpenReplayFolder();
    }

    private async void OptionsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        var optionsDialog = new OptionsDialog();
        await (optionsDialog.ShowDialog(this));
    }

    private async void StatisticsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (_mainWindowViewModel.IsPlayerNameSet || 
            !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PlayerName"]) &&
             _analyzer.IsDoneAnalyzing) {
            MatchesDataGrid.IsVisible = false;
            UpdateStatisticsTabVisibility(true);
            return;
        }
        var messageBox = new MessageBox(_analyzer.IsDoneAnalyzing ? "Set a player name!" : "Wait for analyzer!", "Ok");
        await(messageBox.ShowDialog(this));
    }
    
    private void ExitMenuItem_Click(object? sender, RoutedEventArgs e) => Close();

    private void TableMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        MatchesDataGrid.IsVisible = true;
        UpdateStatisticsTabVisibility(false);
    }

    private void UpdateStatisticsTabVisibility(bool isVisible)
    {
        StatisticsGrid.IsVisible = isVisible;
        StatisticsPlot.IsVisible = isVisible;
    }

    private void UpdateProgressBar(int count, int max)
    {
        if (count == 0)
            (new Action(() => SRRAProgressBar.IsVisible = true)).Invoke();
        if (count == max)
            (new Action(() => SRRAProgressBar.IsVisible = false)).Invoke();

        new Action(() => SRRAProgressBar.Value = (++count * 100) / max).Invoke();
    }
}

