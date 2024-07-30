using System;

namespace PixelRuler.Models
{
    public class ScreenshotInfo
    {
        public DateTime DateTime { get; set; }
        public int Width { get; set; }
        public int Height {get; set; }
        public string WindowTitle {get; set; }
        public string ProcessName {get; set; }
        public string? LastSavedPath {get; set; }

        //public ScreenshotInfo(DateTime dateTime, int width, int height, string windowTitle, string processName) 
        //{ 
        //}
    }
}
