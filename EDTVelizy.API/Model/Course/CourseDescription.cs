using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDTVelizy.API;

[Serializable]
public class CourseDescription
{
    [JsonPropertyName("professor")] public string Professor { get; set; }
    [JsonPropertyName("room")] public string Room { get; set; }
    [JsonPropertyName("subject")] public string Subject { get; set; }
    [JsonPropertyName("group")] public string Group { get; set; }
    [JsonPropertyName("event_type")] public string EventType { get; set; }
    [JsonPropertyName("note")] public string Note { get; set; }
    [JsonPropertyName("hour")] public string Hour { get; set; }
}