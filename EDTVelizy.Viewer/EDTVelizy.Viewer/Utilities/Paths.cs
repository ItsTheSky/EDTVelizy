using System;
using System.Linq;

namespace EDTVelizy.Viewer.Utilities;

/// <summary>
/// Utilities methods for 
/// </summary>
public static class Paths
{
    
    public static string Build(params string[] paths)
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var path = System.IO.Path.Combine(folder, "EDTVelizy");

        return paths.Aggregate(path, System.IO.Path.Combine);
    }
    
}