using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Dialogs;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EDTVelizy.API;
using EDTVelizy.Viewer.Controls;
using EDTVelizy.Viewer.Converters;
using EDTVelizy.Viewer.Views;

namespace EDTVelizy.Viewer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty] private ObservableCollection<ScheduleItem> _items = [];
    [ObservableProperty] private Dictionary<DateOnly, List<InternalCourse>> _loadedCoursesCache = new();
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _settingsOpened = false;
    [ObservableProperty] private bool _isDailySchedule = false; 
    
    public ObservableCollection<DayOfTheWeek> DaysOfWeek { get; } =
    [
        new (DayOfWeek.Monday, "L"),
        new (DayOfWeek.Tuesday, "M"),
        new (DayOfWeek.Wednesday, "M"),
        new (DayOfWeek.Thursday, "J"),
        new (DayOfWeek.Friday, "V"),
        new (DayOfWeek.Saturday, "S", true),
        new (DayOfWeek.Sunday, "D", true)
    ];

    public record DayOfTheWeek(DayOfWeek Day, string Letter, bool IsWeekend = false)
    {
        public AsyncRelayCommand SelectDay => new AsyncRelayCommand(() =>
        {
            MainView.Instance.ViewModel.GoToDayOfTheWeekCommand.Execute(Day);
            return Task.CompletedTask;
        });
    };

    private InternalCourse? _showingCourse = null;
    public InternalCourse? ShowingCourse
    {
        get => _showingCourse;
        set
        {
            SetProperty(ref _showingCourse, value);
            if (value != null)
                MainView.Instance.CourseCardControl.SetCourse(value);
        }
    }
    
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Now);

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            SetProperty(ref _selectedDate, value);
            OnPropertyChanged(nameof(DatePickerDate));
        }
    }

    public DateOnly DatePickerDate
    {
        get => SelectedDate;
        set
        {
            SelectedDate = value;
            Dispatcher.UIThread.InvokeAsync(MoveToDay);
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    public async Task GoToToday()
    {
        SelectedDate = DateOnly.FromDateTime(DateTime.Now);
        if (Settings.BetterToday && DateTime.Now.Hour >= 18)
            SelectedDate = SelectedDate.AddDays(1);
        await MoveToDay();
    }

    [RelayCommand]
    public async Task PreviousDay()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await MoveToDay();
    }
    
    [RelayCommand]
    public async Task NextDay()
    {
        SelectedDate = SelectedDate.AddDays(1);
        await MoveToDay();
    }

    [RelayCommand]
    public async Task GoToDayOfTheWeek(DayOfWeek dayOfWeek)
    {
        SelectedDate = SelectedDate.AddDays((int) dayOfWeek - (int) SelectedDate.DayOfWeek);
        await MoveToDay();
    }

    private async Task MoveToDay()
    {
        Items.Clear();

        List<InternalCourse> courses;
        if (LoadedCoursesCache.TryGetValue(SelectedDate, out List<InternalCourse>? value))
        {
            courses = value;
        }
        else
        {
            IsLoading = true;
            var dayIndex = ActualDayOfTheWeek(SelectedDate.DayOfWeek);
            var startOfWeek = SelectedDate.AddDays(- dayIndex);
            var endOfWeek = startOfWeek.AddDays(6);
            var req = new CalendarRequest
            {
                StartDate = startOfWeek,
                EndDate = endOfWeek,
                ColourScheme = "3",
                ViewType = CalendarRequest.CalendarViewType.AgendaWeek,
                FederationId = Settings.Group
            };

            courses = [];
            var tokenSource = new CancellationTokenSource(new TimeSpan(0, 0, 5));
            try
            { 
                var loadedCourses = await Endpoints.GetCourses(req, tokenSource.Token);
                if (loadedCourses.Count == 0)
                {
                    NotificationManager.Show(
                        new Notification("Faute de frappe?", 
                            "Aucun cours n'a été trouvé pour cette semaine. Vérifiez que le groupe est correctement configuré."),
                        type: NotificationType.Warning,
                        classes: ["Light"]);
                }
                
                var sortedCourses = new Dictionary<DateOnly, List<InternalCourse>>();
                foreach (var course in loadedCourses)
                {
                    var internalCourse = new InternalCourse
                    {
                        Course = course,
                        Description = new CourseDescription()
                    };
                    _ = Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        internalCourse.Description = await internalCourse.Course.GetDescription(token: tokenSource.Token);
                
                        var found = Items.ToList().Find(value => value.Course == internalCourse);
                        if (found == null)
                            return;
                        
                        found.Color = () => Settings.BetterColors 
                            ? CourseTypeToColorConverter.GetColorForType(internalCourse.Description.EventType) 
                            : new SolidColorBrush(course.BackgroundColor.ToAvaloniaColor());
                        MainView.Instance.UpdateVisual();
                    });
                    
                    var dateTime = DateOnly.FromDateTime(internalCourse.Course.Start.Date);
                    if (!sortedCourses.ContainsKey(dateTime))
                        sortedCourses[dateTime] = [];
                    sortedCourses[dateTime].Add(internalCourse);
                    if (dateTime == SelectedDate)
                        courses.Add(internalCourse);
                }

                var processedDays = new List<DateOnly>();
                foreach (var (date, courseList) in sortedCourses)
                {
                    LoadedCoursesCache[date] = courseList;
                    processedDays.Add(date);
                }
                
                Console.WriteLine("Processed " + processedDays.Count + " days");
                foreach (var day in DaysOfWeek)
                {
                    if (!processedDays.Contains(SelectedDate.AddDays(ActualDayOfTheWeek(day.Day) - ActualDayOfTheWeek(SelectedDate.DayOfWeek)))
                        && day.Day != SelectedDate.DayOfWeek)
                    {
                        LoadedCoursesCache[SelectedDate.AddDays(ActualDayOfTheWeek(day.Day) - ActualDayOfTheWeek(SelectedDate.DayOfWeek))] = [];
                    }
                }
                
                MainView.CacheManager.UpdateCache();
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                {
                    NotificationManager.Show(
                        new Notification("Requête annulée", 
                            "La requête a pris trop de temps à répondre. Veuillez réessayer."),
                        type: NotificationType.Error,
                        classes: ["Light"]);
                }
                else
                {
                    NotificationManager.Show(
                        new Notification("Erreur", 
                            "Une erreur est survenue lors de la récupération des cours. Veuillez réessayer."),
                        type: NotificationType.Error,
                        classes: ["Light"]);
                }
                
                Console.WriteLine(e);
            } finally
            {
                IsLoading = false;
            }
        }
        
        foreach (var internalCourse in courses)
        {
            var scheduleItem = new ScheduleItem
            {
                Course = internalCourse,
                StartTime = internalCourse.Course.Start.TimeOfDay,
                EndTime = internalCourse.Course.End.TimeOfDay,
                Color = () => Settings.BetterColors 
                    ? CourseTypeToColorConverter.GetColorForType(internalCourse.Description.EventType) 
                    : new SolidColorBrush(internalCourse.Course.BackgroundColor.ToAvaloniaColor()),
                CreateContent = () => new CourseCard(internalCourse),
                ClickCommand = new AsyncRelayCommand(async () =>
                {
                    ShowingCourse = internalCourse;
                })
            };
            Items.Add(scheduleItem);
        }
        
        var sorted = Items.ToList();
        sorted.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        
        MainView.Instance.UpdateVisual();
        // we also have to update DaysOfWeek to trigger the DayOfWeekToButtonClassConverter
        var temp = new ObservableCollection<DayOfTheWeek>(DaysOfWeek);
        DaysOfWeek.Clear();
        foreach (var day in temp)
            DaysOfWeek.Add(day);
    }

    [RelayCommand]
    public void CloseCourse()
    {
        ShowingCourse = null;
    }
    
    [RelayCommand]
    public void OpenSettings()
    {
        MainView.Instance.Settings.LoadSettings();
        SettingsOpened = true;
    }

    #endregion

    public static int ActualDayOfTheWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            DayOfWeek.Sunday => 6,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public partial class InternalCourse : ObservableObject
    {

        [ObservableProperty] private Course _course;
        [ObservableProperty] private CourseDescription _description;
        
        public string StartTime => Course.Start.ToString("HH:mm");
        public string EndTime => Course.End.ToString("HH:mm");
    }
    
    public static SettingsViewModel.SettingsModel Settings
    {
        get
        {
            MainView.Instance.Settings.LoadSettings();
            
            return MainView.Instance.Settings.ViewModel.Settings;
        }
    }
    
    public static WindowNotificationManager NotificationManager => MainView.Instance.NotificationManager!;
}