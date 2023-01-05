using Avalonia.Controls;
using System;
using System.Configuration;

namespace srra
{
    public partial class OptionsDialog : Window, IDisposable
    {
        public OptionsDialog()
        {
            InitializeComponent();
            SetSCREPPathButton.Click += SetSCREPPathButton_Click;
            SetReplayPathButton.Click += SetReplayPathButton_Click;
        }

        private void SetSCREPPathButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SetPath("SCREP_Path");
        }

        private void SetReplayPathButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SetPath("Replay_Path");
        }

        public async void SetPath(string pathToSet)
        {
            var ofd = new OpenFolderDialog() {
                Title = $"Select {pathToSet.Replace('_', ' ')}",
            };
            var path = await ofd.ShowAsync(this);
            if (path != null) {
                SaveConfig(pathToSet, path);
            }
        }

        public static void SaveConfig(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public void Dispose()
        {
            Close();
        }
    }
}
