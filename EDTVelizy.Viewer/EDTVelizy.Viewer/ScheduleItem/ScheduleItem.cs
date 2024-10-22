using System;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using EDTVelizy.Viewer.ViewModels;

namespace EDTVelizy.Viewer;

public class ScheduleItem
{
    
    public MainViewModel.InternalCourse Course { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Func<Control> CreateContent { get; set; }
    public Func<IBrush> Color { get; set; }
    public AsyncRelayCommand ClickCommand { get; set; }
    
}