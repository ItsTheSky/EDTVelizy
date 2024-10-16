using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDTVelizy.API;

public class CourseDescription
{
    public string Professor { get; set; }
    public string Room { get; set; }
    public string Subject { get; set; }
    public string Group { get; set; }
    public string EventType { get; set; }
    public string Note { get; set; }
    public string Hour { get; set; }
}