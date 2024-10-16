using System.Text.Json;
using System.Text.Json.Serialization;
using EDTVelizy.API;
using EDTVelizy.Core;

namespace EDTVelizy.Demo;

class Program
{
    static async Task Main(string[] args)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var req = new CalendarRequest
        {
            StartDate = today,
            EndDate = today,
            ColourScheme = "3",
            ViewType = CalendarRequest.CalendarViewType.AgendaWeek
        };
        
        var courses = await Endpoints.GetCourses(req);
        // find the current course
        Course? currentCourse = null;
        foreach (var course in courses)
        {
            if (course.Start <= DateTime.Now && course.End >= DateTime.Now)
            {
                currentCourse = course;
                break;
            }
        }
        
        if (currentCourse == null)
        {
            Console.WriteLine("No course is currently happening.");
            return;
        }
        
        Console.WriteLine("Current course:");
        var desc = await currentCourse.GetDescription();
        Console.WriteLine($"- {desc.Subject} in {desc.Room} with {desc.Professor} (type: {desc.EventType})");
        Console.WriteLine($"- Starting at {currentCourse.Start.TimeOfDay} and ending at {currentCourse.End.TimeOfDay}");
        
    }
}