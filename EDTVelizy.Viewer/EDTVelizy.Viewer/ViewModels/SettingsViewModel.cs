using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EDTVelizy.Viewer.Views;

namespace EDTVelizy.Viewer.ViewModels;

public partial class SettingsViewModel : ObservableObject
{

    [ObservableProperty] private bool _needSaving = false;
    
    [ObservableProperty] private SettingsModel _settings = new ();
    [ObservableProperty] private SettingsModel _settingsBackup = new ();

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

    #endregion
    
    [Serializable]
    public partial class SettingsModel : ObservableObject
    {
        
        [ObservableProperty]
        [JsonPropertyName("group")]
        private string _group = "INF1-B";

        #region Clone
        
        public SettingsModel Clone()
        {
            return new SettingsModel
            {
                Group = Group
            };
        }

        #endregion
    }

}