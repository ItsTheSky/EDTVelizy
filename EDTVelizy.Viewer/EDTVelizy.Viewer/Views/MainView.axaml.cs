using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using EDTVelizy.Viewer.ViewModels;

namespace EDTVelizy.Viewer.Views;

public partial class MainView : UserControl
{
    public static MainView Instance { get; private set; }
    
    public MainView()
    {
        InitializeComponent();
        
        Instance = this;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
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