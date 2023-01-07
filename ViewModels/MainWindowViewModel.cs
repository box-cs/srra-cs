using DynamicData;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace srra.ViewModels
{
    public class MainWindowViewModel
    {
        public ObservableCollection<Match> Matches { get; set; } = new();
        public bool IsPlayerNameSet { get; set; }

        public void RefreshDataGrid(List<Match> matches)
        {
            Matches.Clear();
            Matches.AddRange(matches);
        }
    }
}
