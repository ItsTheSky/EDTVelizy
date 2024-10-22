using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using EDTVelizy.API;
using EDTVelizy.Viewer.Views;

namespace EDTVelizy.Viewer.ViewModels;

public partial class SettingsViewModel : ObservableObject
{

    [ObservableProperty] private bool _needSaving;
    
    [ObservableProperty] private SettingsModel _settings = new ();
    [ObservableProperty] private SettingsModel _settingsBackup = new ();
    
    [ObservableProperty] private ObservableCollection<string> _foundGroups = new ();

    private string _groupRequest;
    public string GroupRequest
    {
        get => _groupRequest;
        set
        {
            SetProperty(ref _groupRequest, value);
            if (!FoundGroups.Contains(value))
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var founds = await Endpoints.GetGroups(new FederationRequest
                    {
                        SearchTerm = value 
                    });
                
                    FoundGroups.Clear();
                    FoundGroups.AddRange(founds);
                });
            }
        }
    }

    #region Cache

    public static ObservableCollection<CachePolicyModel> Caches =>
    [
        new(CachePolicy.None, "Aucune"),
        new(CachePolicy.Week, "Semaine"),
        new(CachePolicy.All, "Tout")
    ];
    private CachePolicyModel _selectedCache = Caches[0];
    public CachePolicyModel SelectedCache
    {
        get => _selectedCache;
        set
        {
            if (value == null! || MainView.CacheManager == null!) 
                return;
            
            SetProperty(ref _selectedCache, value);
            Settings.CachePolicy = value.Policy;
            NeedSaving = true;
            
            MainView.CacheManager.UpdateCache();
        }
    }

    public record CachePolicyModel(CachePolicy Policy, string DisplayName);
    public enum CachePolicy
    {
        None, Week, All
    }
    
    #endregion
    
    #region Commands

    [RelayCommand]
    public void SaveSettings()
    {
        NeedSaving = false;
        var hasGroupChanged = Settings.Group != SettingsBackup.Group;
        SettingsBackup = Settings.Clone();

        if (hasGroupChanged)
        {
            MainView.Instance.ViewModel.LoadedCoursesCache.Clear();
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MainView.Instance.ViewModel.GoToToday();
            });
        }

        MainView.Instance.Settings.SaveSettings();
        MainView.Instance.ViewModel.SettingsOpened = false;
    }
    
    [RelayCommand]
    public void CloseSettings()
    {
        Settings = SettingsBackup.Clone();
        NeedSaving = false;

        MainView.Instance.ViewModel.SettingsOpened = false;
    }
    
    [RelayCommand]
    public void HelpPressed(string rawIndex)
    {
        var index = int.Parse(rawIndex);
        var help = index switch
        {
            0 => "Groupe à utiliser pour récupérer les cours",
            1 => """
                 Comment les cours récupérés sont mis en cache par l'application:
                 • Aucune: aucune mise en cache
                 • Semaine: les cours de la semaine en cours sont mis en cache
                 • Tout: tous les cours sont mis en cache
                 (Les cours mis en cache sont accessibles hors-ligne)
                 """,
            2 => "Si l'heure actuelle est après 18h, le jour affiché par défaut sera le lendemain",
            _ => ""
        };
        
        MainView.Instance.NotificationManager?.Show(
            new TextBlock { Text = help, TextWrapping = TextWrapping.Wrap },
            type: NotificationType.Information,
            classes: ["Light"]);
    }

    [RelayCommand]
    public void ClearCache()
    {
        
        MainView.Instance.ViewModel.LoadedCoursesCache.Clear();
        MainView.CacheManager.UpdateCache();
        Dispatcher.UIThread.InvokeAsync(async () => await MainView.Instance.ViewModel.GoToToday());
        
        MainView.Instance.NotificationManager?.Show(
            new Notification("Cache vidé", 
                "Le cache a été vidé avec succès"),
            type: NotificationType.Information,
            classes: ["Light"]);
        
    }

    #endregion
    
    [Serializable]
    public partial class SettingsModel : ObservableObject
    {
        
        [ObservableProperty]
        [JsonPropertyName("group")]
        private string _group = "INF1-B";
        
        [ObservableProperty]
        [JsonPropertyName("cache_policy")]
        private CachePolicy _cachePolicy = CachePolicy.Week;
        
        [ObservableProperty]
        [JsonPropertyName("better_today")]
        private bool _betterToday = true;

        #region Clone
        
        public SettingsModel Clone()
        {
            return new SettingsModel
            {
                Group = Group,
                CachePolicy = CachePolicy,
                BetterToday = BetterToday
            };
        }

        #endregion
    }

}