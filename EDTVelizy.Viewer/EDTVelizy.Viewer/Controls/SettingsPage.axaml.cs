using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Threading;
using EDTVelizy.API;
using EDTVelizy.Viewer.ViewModels;
using Path = System.IO.Path;

namespace EDTVelizy.Viewer.Controls;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
        
        DataContext = new SettingsViewModel();
    }

    private bool _loaded;
    public void LoadSettings()
    {
        if (_loaded)
            return;

        var dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settingsFile = Path.Join(dataFolder, "settings.json");
        if (File.Exists(settingsFile))
        {
            Console.WriteLine("Settings file found, loading settings");
            var settings = JsonSerializer.Deserialize<SettingsViewModel.SettingsModel>(File.ReadAllText(settingsFile))
                           ?? new SettingsViewModel.SettingsModel();
            
            ViewModel.Settings = settings.Clone();
            ViewModel.SettingsBackup = settings.Clone();
        }
        else
        {
            Console.WriteLine("Settings file not found, creating new settings");
            ViewModel.Settings = new SettingsViewModel.SettingsModel();
            ViewModel.SettingsBackup = new SettingsViewModel.SettingsModel();
        }
        
        ViewModel.NeedSaving = false;
        ViewModel.SelectedCache = SettingsViewModel.Caches.First(c => c.Policy == ViewModel.Settings.CachePolicy);
        
        _loaded = true;
    }

    public void SaveSettings()
    {
        var dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settingsFile = Path.Join(dataFolder, "settings.json");
        File.WriteAllText(settingsFile, JsonSerializer.Serialize(ViewModel.Settings));
    }
    
    public SettingsViewModel ViewModel => (SettingsViewModel) DataContext!;
}