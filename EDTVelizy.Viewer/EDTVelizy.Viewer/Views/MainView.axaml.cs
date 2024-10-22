using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using EDTVelizy.API;
using EDTVelizy.Viewer.Utilities;
using EDTVelizy.Viewer.ViewModels;
using Path = System.IO.Path;

namespace EDTVelizy.Viewer.Views;

public partial class MainView : UserControl
{
    public static MainView Instance { get; private set; }
    public static CacheManager CacheManager;
    
    public MainView()
    {
        InitializeComponent();
        
        Instance = this;
        CacheManager = new CacheManager(this);
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            CacheManager.LoadCache();
            Settings.LoadSettings();
            
            await ViewModel.GoToToday();
        });
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        var topLevel = TopLevel.GetTopLevel(this);
        NotificationManager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 4,
            Position = NotificationPosition.TopCenter,
        };
    }

    public WindowNotificationManager? NotificationManager { get; private set; }
    public MainViewModel ViewModel => (MainViewModel) DataContext!;
    public TopLevel TopLevel => TopLevel.GetTopLevel(this)!;
}