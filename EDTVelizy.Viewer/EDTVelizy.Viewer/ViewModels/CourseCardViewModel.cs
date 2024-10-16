using CommunityToolkit.Mvvm.ComponentModel;

namespace EDTVelizy.Viewer.ViewModels;

public partial class CourseCardViewModel : ObservableObject
{

    [ObservableProperty] private MainViewModel.InternalCourse? _course;

}