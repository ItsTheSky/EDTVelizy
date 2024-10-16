using System.Drawing;
using System.Text.Json.Serialization;

namespace EDTVelizy.API;

/*
 *     {
        "id": "-746998998:-2064558780:7:15220:7",
        "start": "2024-10-17T13:00:00",
        "end": "2024-10-17T16:00:00",
        "allDay": false,
        "description": "CUKALLA Etleva<br />OSTER Alain\r\n\r\n<br />\r\n\r\nINF1-B\r\n\r\n<br />\r\n\r\n313 - VEL\r\n\r\n<br />\r\n\r\nR1.06 - Mathematiques discretes<br />R1.10 - Anglais\r\n",
        "backgroundColor": "#8000FF",
        "textColor": "#ffffff",
        "department": "INFO",
        "faculty": "IUT de Vélizy",
        "eventCategory": "Travaux Pratiques (TP)",
        "sites": [
            "Bat. MERMOZ - VEL"
        ],
        "modules": [
            "IN1R06",
            "IN1R10"
        ],
        "registerStatus": 0,
        "studentMark": 0,
        "custom1": null,
        "custom2": null,
        "custom3": null
    },
 */
[Serializable]
public class Course
{

    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("start")] public DateTime Start { get; set; }
    [JsonPropertyName("end")] public DateTime End { get; set; }
    [JsonPropertyName("allDay")] public bool AllDay { get; set; }
    
    [JsonPropertyName("backgroundColor")] public Color BackgroundColor { get; set; }
    [JsonPropertyName("textColor")] public Color TextColor { get; set; }
    [JsonPropertyName("department")] public string Department { get; set; }
    [JsonPropertyName("faculty")] public string Faculty { get; set; }
    [JsonPropertyName("eventCategory")] public string EventCategory { get; set; }
    [JsonPropertyName("sites")] public List<string> Sites { get; set; }
    [JsonPropertyName("modules")] public List<string> Modules { get; set; }
    [JsonPropertyName("registerStatus")] public int RegisterStatus { get; set; }
    [JsonPropertyName("studentMark")] public int StudentMark { get; set; }
    
    [JsonPropertyName("custom1")] public string? Custom1 { get; set; }
    [JsonPropertyName("custom2")] public string? Custom2 { get; set; }
    [JsonPropertyName("custom3")] public string? Custom3 { get; set; }
}