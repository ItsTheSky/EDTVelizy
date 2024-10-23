using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using EDTVelizy.Viewer.Views;

namespace EDTVelizy.Viewer.Controls
{
    public class ScheduleListControl : UserControl, IScheduleControl
    {

        public static readonly StyledProperty<IEnumerable<ScheduleItem>> ItemsProperty =
            AvaloniaProperty.Register<ScheduleListControl, IEnumerable<ScheduleItem>>(nameof(Items));

        public IEnumerable<ScheduleItem> Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        private readonly StackPanel _mainStack;

        public ScheduleListControl()
        {
            _mainStack = new StackPanel
            {
                Spacing = 5,
                Margin = new Thickness(10)
            };

            Content = _mainStack;

            this.GetObservable(ItemsProperty).Subscribe(_ => UpdateVisual());
        }

        public void UpdateVisual()
        {
            _mainStack.Children.Clear();

            if (Items == null!)
                return;

            if (!Items.Any())
            {
                var noItemsBlock = new TextBlock
                {
                    Text = "Aucun cours trouvé",
                    FontSize = 18,
                    Foreground = Brushes.Gray
                };

                _mainStack.Children.Add(noItemsBlock);
                return;
            }
            
            // sort items per start times
            var items = Items.OrderBy(x => x.StartTime).ToList();
            foreach (var item in items)
            {
                var itemContainer = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,10,*"),
                    Margin = new Thickness(0, 5),
                    Cursor = new Cursor(StandardCursorType.Hand)
                };
                
                itemContainer.Tapped += (sender, e) =>
                {
                    if (item.Course != null!)
                        MainView.Instance.ViewModel.ShowingCourse = item.Course;
                };

                // Colonne de temps
                var timeStack = new Grid
                {
                    Width = 50,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    RowDefinitions = new RowDefinitions("*,*")
                };

                var startTimeBlock = new TextBlock
                {
                    Text = $"{item.StartTime:hh\\:mm}",
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Top
                };

                var endTimeBlock = new TextBlock
                {
                    Text = $"{item.EndTime:hh\\:mm}",
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    FontWeight = FontWeight.DemiBold
                };

                timeStack.Children.Add(startTimeBlock);
                timeStack.Children.Add(endTimeBlock);
                Grid.SetRow(startTimeBlock, 0);
                Grid.SetRow(endTimeBlock, 1);

                // Barre verticale colorée
                var colorBar = new Border
                {
                    Background = item.Color.Invoke(),
                    Width = 4,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    CornerRadius = new CornerRadius(2)
                };

                // Contenu du cours
                var baseColor = Application.Current?.FindResource("SemiGrey8Color") is Color
                    ? (Color) Application.Current?.FindResource("SemiGrey8Color")!
                    : Colors.Gray;
                var currentColor = Application.Current?.FindResource("SemiIndigo8Color") is Color
                    ? (Color) Application.Current?.FindResource("SemiIndigo8Color")!
                    : Colors.Crimson;

                var isCurrentHour = item.StartTime <= DateTime.Now.TimeOfDay && item.EndTime >= DateTime.Now.TimeOfDay;
                // be sur eit's the same day too
                if (DateTime.Now.DayOfYear != item.Course.Course.Start.DayOfYear)
                    isCurrentHour = false;
                
                var contentBorder = new Border
                {
                    Background = new SolidColorBrush(isCurrentHour ? currentColor : baseColor),
                    Padding = new Thickness(10),
                    CornerRadius = new CornerRadius(4)
                };

                var contentGrid = new Grid
                {
                    RowDefinitions = new RowDefinitions("Auto,Auto,Auto")
                };

                // Titre du cours
                var titleBlock = new TextBlock
                {
                    Text = item.Course.Description.DisplayedSubjects,
                    FontSize = 18,
                    FontWeight = FontWeight.Bold,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };


                // Informations du professeur et de la salle
                var profRoomStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 5
                };

                var professorBlock = new TextBlock
                {
                    Text = item.Course.Description.DisplayedProfessors,
                    FontSize = 14,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                var separatorBlock = new TextBlock
                {
                    Text = "•",
                    FontSize = 14,
                    Margin = new Thickness(5, 0)
                };

                //<Label Classes="Purple" Theme="{StaticResource TagLabel}">Purple</Label>
                var roomBlock = new Label
                {
                    Content = item.Course.Description.DisplayedRooms,
                    FontSize = 14,
                    Classes = { "Orange" },
                    Theme = Application.Current!.FindResource("TagLabel") as ControlTheme,
                };

                profRoomStack.Children.Add(roomBlock);
                profRoomStack.Children.Add(separatorBlock);
                profRoomStack.Children.Add(professorBlock);

                // Groupe
                var groupBlock = new TextBlock
                {
                    Text = item.Course.Description.DisplayedGroups,
                    FontSize = 14,
                    Foreground = Brushes.Gray
                };

                contentGrid.Children.Add(titleBlock);
                Grid.SetRow(titleBlock, 0);

                contentGrid.Children.Add(profRoomStack);
                Grid.SetRow(profRoomStack, 1);

                contentGrid.Children.Add(groupBlock);
                Grid.SetRow(groupBlock, 2);

                contentBorder.Child = contentGrid;

                // Assemblage final
                Grid.SetColumn(timeStack, 0);
                Grid.SetColumn(colorBar, 1);
                Grid.SetColumn(contentBorder, 2);

                itemContainer.Children.Add(timeStack);
                itemContainer.Children.Add(colorBar);
                itemContainer.Children.Add(contentBorder);

                _mainStack.Children.Add(itemContainer);
            }
        }
    }
}