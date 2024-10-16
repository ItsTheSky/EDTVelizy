using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EDTVelizy.API;

namespace EDTVelizy.Viewer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty] private ObservableCollection<InternalCourse> _loadedCourses = [];

    #endregion

    #region Commands

    [RelayCommand]
    public async Task ReloadCourses()
    {
        LoadedCourses.Clear();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var req = new CalendarRequest
        {
            StartDate = today,
            EndDate = today,
            ColourScheme = "3",
            ViewType = CalendarRequest.CalendarViewType.AgendaWeek
        };
        
        var courses = await Endpoints.GetCourses(req);
        foreach (var course in courses)
        {
            var internalCourse = new InternalCourse
            {
                Course = course,
                Description = await course.GetDescription()
            };
            
            LoadedCourses.Add(internalCourse);
        }
    } 

    #endregion

    public partial class InternalCourse : ObservableObject
    {

        [ObservableProperty] private Course _course;
        [ObservableProperty] private CourseDescription _description;
        
        public string StartTime => Course.Start.ToString("HH:mm");
        public string EndTime => Course.End.ToString("HH:mm");
    }
}