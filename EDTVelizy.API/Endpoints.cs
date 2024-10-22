using System.Text.Json;
using System.Text.Json.Nodes;
using EDTVelizy.Core;

namespace EDTVelizy.API;

/// <summary>
/// List of commonly-used endpoints with direct JSON serialization.
/// </summary>
public static class Endpoints
{

    public static async Task<List<Course>> GetCourses(CalendarRequest request, CancellationToken token = default)
    {
        var response = await RequestUtils.PostAsync("GetCalendarData", request, token);
        var content = await response.Content.ReadAsStringAsync();
        return JsonUtils.Deserialize<List<Course>>(content);
    }

    public static async Task<CourseDescription> GetDescription(this Course course, CancellationToken token = default)
    {
        var obj = new { eventId = course.Id };
        var response = await RequestUtils.PostAsync("GetSideBarEvent", obj, token);
        var content = await response.Content.ReadAsStringAsync(token);
        var elementsJson = JsonDocument.Parse(content).RootElement.GetProperty("elements");
        var description = new CourseDescription();

        string? lastElementType = null;
        foreach (var element in elementsJson.EnumerateArray())
        {
            var key = element.GetProperty("label").GetString();
            if (key == null && lastElementType != null)
                key = lastElementType;
            else
                lastElementType = key;
            
            var value = element.GetProperty("content").ValueKind == JsonValueKind.Array 
                ? string.Join(", ", element.GetProperty("content").EnumerateArray().Select(e => e.GetString())) 
                : element.GetProperty("content").GetString();
            switch (key)
            {
                case "Personnel":
                    description.Professors.Add(value);
                    break;
                case "Salle":
                    description.Rooms.Add(value);
                    break;
                case "Matière":
                case "Matières":
                    description.Subjects.Add(value);
                    break;
                case "Groupe":
                    description.Groups.Add(value);
                    break;
                case "Catégorie d’événement":
                    description.EventTypes.Add(value);
                    break;
                case "Remarques":
                    description.Notes.Add(value);
                    break;
                case "Heure":
                    description.Hours.Add(value);
                    break;
            }
        }
        
        return description;
    }
    
    public static async Task<List<string>> GetGroups(FederationRequest request, CancellationToken token = default)
    {
        var response = await RequestUtils.PostAsync("ReadResourceListItems", request, token);
        var content = await response.Content.ReadAsStringAsync(token);
        Console.WriteLine(content);
        var json = JsonDocument.Parse(content);
        var results = json.RootElement.GetProperty("results");
        var groups = new List<string>();
        foreach (var group in results.EnumerateArray())
            groups.Add(group.GetProperty("text").GetString());
        return groups;
    }
    
}