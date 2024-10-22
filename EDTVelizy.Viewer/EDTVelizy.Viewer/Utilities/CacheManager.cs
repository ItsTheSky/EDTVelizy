using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using EDTVelizy.Viewer.ViewModels;
using EDTVelizy.Viewer.Views;

namespace EDTVelizy.Viewer.Utilities;

public class CacheManager(MainView Instance)
{

    private Dictionary<DateOnly, List<MainViewModel.InternalCourse>> CourseCache
        => Instance.ViewModel.LoadedCoursesCache;

    public void UpdateCache()
    {
        var policy = MainView.Instance.Settings.ViewModel.Settings.CachePolicy;
        var file = Paths.Build("cache.json");
        if (policy == SettingsViewModel.CachePolicy.None)
        {
            if (File.Exists(file))
                File.Delete(file);

            return;
        }

        var today = DateTime.Now;
        today = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
        
        var dayIndex = MainViewModel.ActualDayOfTheWeek(today.DayOfWeek);
        var startOfWeek = today.AddDays(- dayIndex);
        
        var endOfWeek = startOfWeek.AddDays(6);
        endOfWeek = new DateTime(endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59);
        
        Console.WriteLine($"Updating cache for {startOfWeek} to {endOfWeek} with policy {policy}");
        
        var coursesToCache = new List<MainViewModel.InternalCourse>();
        coursesToCache.AddRange(policy == SettingsViewModel.CachePolicy.All
            ? CourseCache.Values.SelectMany(x => x)
            : CourseCache.Values.SelectMany(x => x)
                .Where(x => x.Course.Start.Date >= startOfWeek && x.Course.Start.Date <= endOfWeek));

        if (coursesToCache.Count == 0)
            return;
        
        var dir = Path.GetDirectoryName(file);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        
        var emptyDays = new List<DateOnly>();
        foreach (var date in CourseCache.Keys)
        {
            if (date < DateOnly.FromDateTime(startOfWeek) || date > DateOnly.FromDateTime(endOfWeek))
                emptyDays.Add(date);
        }
        
        var model = new CoursesCacheModel
        {
            Courses = coursesToCache,
            CachedEmptyDays = emptyDays
        };
        
        var json = JsonSerializer.Serialize(model);
        File.WriteAllText(file, json);
        Console.WriteLine($"Cached {coursesToCache.Count} courses ({emptyDays.Count} empty days) from {coursesToCache.First().Course.Start.Date} to {coursesToCache.Last().Course.Start.Date}");
    }

    public void LoadCache()
    {
        var file = Paths.Build("cache.json");
        if (!File.Exists(file))
            return;
        
        var json = File.ReadAllText(file);
        var model = JsonSerializer.Deserialize<CoursesCacheModel>(json);
        if (model == null)
            return;

        foreach (var course in model.Courses)
        {
            var date = DateOnly.FromDateTime(course.Course.Start.Date);
            if (!CourseCache.ContainsKey(date))
                CourseCache[date] = new List<MainViewModel.InternalCourse>();
            
            CourseCache[date].Add(course);
        }
        
        foreach (var date in model.CachedEmptyDays)
        {
            if (!CourseCache.ContainsKey(date))
                CourseCache[date] = new List<MainViewModel.InternalCourse>();
        }
    }
    
    [Serializable]
    public class CoursesCacheModel
    {
        
        [JsonPropertyName("courses")] public List<MainViewModel.InternalCourse> Courses { get; set; } = new();
        [JsonPropertyName("cached_empty_days")] public List<DateOnly> CachedEmptyDays { get; set; } = new(); 
        
    }
}