using srra.Starcraft;
using System.Collections.ObjectModel;

namespace srra.ViewModels
{
    public class MainWindowViewModel
    {
        public ObservableCollection<Match> Matches { get; set; } = new();
        public ObservableCollection<SimpleWinRate> SimpleWinRates { get; set; } = new(); 
        public bool IsPlayerNameSet { get; set; }
    }
}
