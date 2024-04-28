using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler.Models
{
    public class ScreenshotInfo
    {
        public DateTime DateTime;
        public int Width;
        public int Height;
        public string WindowTitle;
        public string ProcessName;
        public string? LastSavedPath;

        //public ScreenshotInfo(DateTime dateTime, int width, int height, string windowTitle, string processName) 
        //{ 
        //}
    }
}
