using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using DynamicData;
using System.Collections.Generic;
using System.Configuration;

namespace srra;

public partial class MainWindow : Window
{
    readonly MainWindowViewModel _mainWindowViewModel;
    readonly Analyzer _analyzer;
    public MainWindow()
    {
        InitializeComponent();
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
        var matches = await MatchLoader.LoadMatches();
        var replayReader = new ReplayReader(this, _mainWindowViewModel, matches);
        await replayReader.ReadReplays();
        _analyzer.UpdateGraphData();
        _analyzer.AnalyzeReplays(new List<Match>(replayReader.replayData));
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
        var optionsDialog = new OptionsDialog();
        await (optionsDialog.ShowDialog(this));
    }

    private void ExitMenuItem_Click(object? sender, RoutedEventArgs e) => Close();

    private async void StatisticsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (_mainWindowViewModel.IsPlayerNameSet || !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PlayerName"])) {
            MatchesDataGrid.IsVisible = false;
            StatisticsGrid.IsVisible = true;
            StatisticsPlot.IsVisible = true;
            return;
        }
        var messageBox = new MessageBox("Set a player name!", "Ok");
        await(messageBox.ShowDialog(this));
    }

    private void TableMenuItem_Click(object? sender, RoutedEventArgs e)
    {
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
    public bool IsPlayerNameSet { get; set; }
    public WinRates WinRates = new WinRates();

    public void RefreshDataGrid(List<Match> matches)
    {
        Matches.Clear();
        Matches.AddRange(matches);
    }
}
