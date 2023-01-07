using Avalonia.Controls;
using Avalonia.Interactivity;
using srra.ViewModels;
using System.Collections.Generic;
using System.Configuration;
using System;
using DynamicData;
using srra.Starcraft;
using srra.Analyzers;
using System.Threading;
using System.Linq;

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
 
    public void ProcessData()
    {
        string? screpPath = ConfigurationManager.AppSettings["SCREP_Path"];
        var replayReader = new ReplayReader(screpPath);
        replayReader.SetReplayPaths();
        var threads = new List<Thread>();

        const int MAX_NUMBER_OF_THREADS = 9;
        Enumerable.Range(0, MAX_NUMBER_OF_THREADS+1).ToList().ForEach(_ => {
            var thread = new Thread((data) => {
                var expectedType = new { chunk = new List<string>(), replayReader = new ReplayReader("") };
                var obj = Helpers.CastTo(data, expectedType);
                var replayReader = obj.replayReader;
                var chunk = obj.chunk;
                chunk.ToList()
                .ForEach(path => {
                    var match = obj.replayReader.ReadReplay(path);
                    if (match != null)
                        replayReader.replayData.Add(match);
                });
            });
            thread.IsBackground = true;
            threads.Add(thread);
        });
        var paths = replayReader.ReplayPaths;
        var chunkedPaths = paths.Chunk(paths.Count > MAX_NUMBER_OF_THREADS ? paths.Count / MAX_NUMBER_OF_THREADS : paths.Count);
        byte count = 0;
        foreach (var chunk in chunkedPaths) {
            threads[count++].Start(new { chunk = chunk.ToList(), replayReader });
        }
        threads.ForEach((thread) => { if (thread.IsAlive) thread.Join();  });
        replayReader.replayData.Sort((a, b) => Nullable.Compare(b.Date, a.Date));
        _mainWindowViewModel.Matches.AddRange(replayReader.replayData);
        _analyzer.UpdateGraphData();
        _analyzer.AnalyzeReplays(replayReader.replayData);
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

