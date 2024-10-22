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
using CommunityToolkit.Mvvm.ComponentModel;
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

            ViewModel.IsDailySchedule = Settings.ViewModel.Settings.DisplayMode;
            await ViewModel.GoToToday();
        });
        
        RegisterEvents();
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

    public void UpdateVisual()
    {
        if (ViewModel.IsDailySchedule)
            DailyScheduleControl.UpdateVisual();
        else
            ListScheduleControl.UpdateVisual();
    }

    #region Drag System

    private Point? _dragStartPoint = null;
    private void RegisterEvents()
    {
        PointerPressed += (sender, e) => _dragStartPoint = e.GetPosition(null);
        PointerReleased += (sender, e) =>
        {
            if (_dragStartPoint == null) 
                return;
            
            var diff = e.GetPosition(null) - _dragStartPoint.Value;
            _dragStartPoint = null;
            
            if (Math.Abs(diff.X) < 20 && Math.Abs(diff.Y) < 20)
                return;
            
            if (diff.X > 0)
                Dispatcher.UIThread.InvokeAsync(ViewModel.PreviousDay);
            else 
                Dispatcher.UIThread.InvokeAsync(ViewModel.NextDay);
        };
    }

    #endregion
}