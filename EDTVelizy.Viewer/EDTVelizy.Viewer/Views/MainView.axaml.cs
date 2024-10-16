using Avalonia.Controls;
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

    public MainViewModel ViewModel => (MainViewModel) DataContext!;
    public TopLevel TopLevel => TopLevel.GetTopLevel(this)!;
}