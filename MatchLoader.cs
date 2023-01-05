using Avalonia.Controls;
using Avalonia.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace srra
{
    public class MatchLoader
    {
        readonly MainWindow _mainWindow;
        readonly MainWindowViewModel _mainWindowVM;
        public MatchLoader(MainWindow mainWindow, MainWindowViewModel mainWindowVM)
        {
            _mainWindow = mainWindow;
            _mainWindowVM = mainWindowVM;
        }

        public async void LoadMatches()
        {
            var progressBar = _mainWindow.SRRAProgressBar;
           progressBar.IsVisible = true;
            var items = Enumerable.Range(0, 100);
            foreach (var item in items) {
                var task = await Task.Run(() => {
                    Task.Delay(150).Wait();
                    return item;
                });
                
                progressBar.Value = item;
            }
            progressBar.IsVisible = false;
        }
    }
}
