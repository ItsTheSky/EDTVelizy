using System.Text.Json;
using System.Text.Json.Nodes;
using EDTVelizy.Core;

namespace EDTVelizy.API;

/// <summary>
/// List of commonly-used endpoints with direct JSON serialization.
/// </summary>
public static class Endpoints
{

    public static async Task<List<Course>> GetCourses(CalendarRequest request)
    {
        var response = await RequestUtils.PostAsync("GetCalendarData", request);
        var content = await response.Content.ReadAsStringAsync();
        return JsonUtils.Deserialize<List<Course>>(content);
    }

    public static async Task<CourseDescription> GetDescription(this Course course)
    {
        var obj = new { eventId = course.Id };
        var response = await RequestUtils.PostAsync("GetSideBarEvent", obj);
        var content = await response.Content.ReadAsStringAsync();
        var elementsJson = JsonDocument.Parse(content).RootElement.GetProperty("elements");
        var description = new CourseDescription();
        
        foreach (var element in elementsJson.EnumerateArray())
        {
            var key = element.GetProperty("label").GetString();
            var value = element.GetProperty("content").GetString();
            switch (key)
            {
                case "Personnel":
                    description.Professor = value;
                    break;
                case "Salle":
                    description.Room = value;
                    break;
                case "Matière":
                    description.Subject = value;
                    break;
                case "Groupe":
                    description.Group = value;
                    break;
                case "Catégorie d’événement":
                    description.EventType = value;
                    break;
                case "Remarques":
                    description.Note = value;
                    break;
                case "Heure":
                    description.Hour = value;
                    break;
            }
        }
        
        return description;
    }
    
}