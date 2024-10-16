using Avalonia.Controls;
using EDTVelizy.Viewer.ViewModels;

namespace EDTVelizy.Viewer.Controls;

public partial class CourseCard : UserControl
{
    
    public CourseCard(MainViewModel.InternalCourse course)
    {
        InitializeComponent();

        DataContext = new CourseCardViewModel
        {
            Course = course
        };
    }
    
    public CourseCard()
    {
        InitializeComponent();
        
        DataContext = new CourseCardViewModel();
    }
    
    public void SetCourse(MainViewModel.InternalCourse course)
    {
        ((CourseCardViewModel) DataContext!).Course = course;
    }

}