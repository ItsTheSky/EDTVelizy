using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
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

    [ObservableProperty] private ObservableCollection<DailyScheduleControl.ScheduleItem> _items = [];
    [ObservableProperty] private Dictionary<DateOnly, List<InternalCourse>> _loadedCoursesCache = new();
    [ObservableProperty] private bool _isLoading = false;
    
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
            // now we'll always retrieve for the whole week of the specified date
            var dayIndex = ActualDayOfTheWeek(SelectedDate.DayOfWeek);
            var startOfWeek = SelectedDate.AddDays(- dayIndex);
            var endOfWeek = startOfWeek.AddDays(6);
            var req = new CalendarRequest
            {
                StartDate = startOfWeek,
                EndDate = endOfWeek,
                ColourScheme = "3",
                ViewType = CalendarRequest.CalendarViewType.AgendaWeek
            };

            courses = [];
            var loadedCourses = await Endpoints.GetCourses(req);
            var sortedCourses = new Dictionary<DateOnly, List<InternalCourse>>();
            foreach (var course in loadedCourses)
            {
                var internalCourse = new InternalCourse
                {
                    Course = course,
                    Description = await course.GetDescription()
                };
                
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
            
            foreach (var day in DaysOfWeek)
            {
                if (!processedDays.Contains(SelectedDate.AddDays(ActualDayOfTheWeek(day.Day) - ActualDayOfTheWeek(SelectedDate.DayOfWeek)))
                    && day.Day != SelectedDate.DayOfWeek)
                {
                    LoadedCoursesCache[SelectedDate.AddDays(ActualDayOfTheWeek(day.Day) - ActualDayOfTheWeek(SelectedDate.DayOfWeek))] = [];
                }
            }
            
            IsLoading = false;
        }
        
        foreach (var internalCourse in courses)
        {
            Items.Add(new DailyScheduleControl.ScheduleItem
            {
                StartTime = internalCourse.Course.Start.TimeOfDay,
                EndTime = internalCourse.Course.End.TimeOfDay,
                Color = CourseTypeToColorConverter.GetColorForType(internalCourse.Description.EventType),
                CreateContent = () => new CourseCard(internalCourse),
                ClickCommand = new AsyncRelayCommand(async () =>
                {
                    ShowingCourse = internalCourse;
                })
            });
        }

        MainView.Instance.ScheduleControl.UpdateScheduleItems();
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

    #endregion

    private static int ActualDayOfTheWeek(DayOfWeek dayOfWeek)
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
}