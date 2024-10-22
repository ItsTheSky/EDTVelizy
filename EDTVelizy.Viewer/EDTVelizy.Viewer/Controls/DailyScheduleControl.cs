using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using EDTVelizy.Viewer.ViewModels;

namespace EDTVelizy.Viewer.Controls
{
    public class DailyScheduleControl : UserControl
    {
        public class ScheduleItem
        {
            public MainViewModel.InternalCourse Course { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public Func<Control> CreateContent { get; set; }
            public IBrush Color { get; set; }
            public AsyncRelayCommand ClickCommand { get; set; }
        }

        public static readonly StyledProperty<IEnumerable<ScheduleItem>> ItemsProperty =
            AvaloniaProperty.Register<DailyScheduleControl, IEnumerable<ScheduleItem>>(nameof(Items));

        public static readonly StyledProperty<bool> ShowHourLinesProperty =
            AvaloniaProperty.Register<DailyScheduleControl, bool>(nameof(ShowHourLines), true);

        public IEnumerable<ScheduleItem> Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public bool ShowHourLines
        {
            get => GetValue(ShowHourLinesProperty);
            set => SetValue(ShowHourLinesProperty, value);
        }

        private Grid _mainGrid;
        private Canvas _scheduleCanvas;
        private const int DisplayedHours = 11;
        private const int StartHour = 8;

        public DailyScheduleControl()
        {
            InitializeComponent();
        }

        public void UpdateScheduleItems()
        {
            _scheduleCanvas.Children.Clear();

            if (Items == null!) 
                return;

            var canvasHeight = _scheduleCanvas.Bounds.Height;
            var hourHeight = canvasHeight / DisplayedHours;

            // Draw hour lines if enabled
            if (ShowHourLines)
            {
                for (int i = 0; i <= DisplayedHours; i++)
                {
                    var y = i * hourHeight;
                    var line = new Line
                    {
                        StartPoint = new Point(0, y),
                        EndPoint = new Point(_scheduleCanvas.Bounds.Width, y),
                        Stroke = Brushes.Gray,
                        StrokeThickness = 1,
                        StrokeDashArray = new AvaloniaList<double> { 2, 2 }
                    };
                    _scheduleCanvas.Children.Add(line);
                }
            }

            foreach (var item in Items)
            {
                var startY = (item.StartTime.TotalHours - StartHour) * hourHeight;
                var endY = (item.EndTime.TotalHours - StartHour) * hourHeight;
                var itemHeight = endY - startY;

                // Assurez-vous que l'élément ne dépasse pas les limites du canvas
                if (startY + itemHeight > canvasHeight)
                {
                    itemHeight = canvasHeight - startY;
                }
                if (startY < 0)
                {
                    itemHeight += startY;
                    startY = 0;
                }

                if (itemHeight <= 0) continue; // Ignore les éléments hors des limites

                var content = item.CreateContent();

                var button = new Border
                {
                    Background = item.Color,
                    Child = content,
                    Width = _scheduleCanvas.Bounds.Width,
                    Height = itemHeight,
                    Padding = new Thickness(5),
                    CornerRadius = new CornerRadius(5),
                    ClipToBounds = true,
                    Theme = Application.Current!.FindResource("SolidButton") as ControlTheme,
                    Cursor = new Cursor(StandardCursorType.Hand),
                    
                    BorderBrush = item.Color.Darken(0.8),
                    BorderThickness = new Thickness(1.5)
                };
                button.Tapped += (sender, args) => item.ClickCommand.Execute(args);

                Canvas.SetTop(button, startY);
                _scheduleCanvas.Children.Add(button);
            }
        }

        private void InitializeComponent()
        {
            _mainGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                RowDefinitions = new RowDefinitions(string.Join(",", Enumerable.Repeat("*", DisplayedHours+1)))
            };

            // Add time labels
            for (int i = 0; i <= DisplayedHours; i++)
            {
                var hour = StartHour + i;
                var timeLabel = new TextBlock
                {
                    Text = $"{hour:D2}h",
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(5, i == 0 ? -3 : -8, 5, 5),
                    Foreground = Brushes.Wheat
                };
                Grid.SetRow(timeLabel, i);
                Grid.SetColumn(timeLabel, 0);
                _mainGrid.Children.Add(timeLabel);
            }

            // Add a Canvas for schedule items
            _scheduleCanvas = new Canvas();
            Grid.SetColumn(_scheduleCanvas, 1);
            Grid.SetRowSpan(_scheduleCanvas, DisplayedHours);
            _mainGrid.Children.Add(_scheduleCanvas);

            Content = _mainGrid;

            this.GetObservable(ItemsProperty).Subscribe(_ => UpdateScheduleItems());
            this.GetObservable(ShowHourLinesProperty).Subscribe(_ => UpdateScheduleItems());
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateScheduleItems();
        }
    }
}