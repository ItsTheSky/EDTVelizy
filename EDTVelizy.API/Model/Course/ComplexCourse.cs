using System.Text.Json.Serialization;

namespace EDTVelizy.API;

[Serializable]
public class ComplexCourse
{
    
    [JsonPropertyName("course")] public Course Course { get; set; }
    [JsonPropertyName("description")] public CourseDescription Description { get; set; }
    
}