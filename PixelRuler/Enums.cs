using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler
{
    public enum DayNightMode
    {
        [DisplayLabel("Follow System")]
        FollowSystem = 0,
        [DisplayLabel("Light")]
        ForceDay = 1,
        [DisplayLabel("Dark")]
        ForceNight = 2,
    }

    public enum OnLaunchAction
    {
        [DisplayLabel("Full Screenshot")]
        FullScreenshot = 0,
        [DisplayLabel("Window Screenshot")]
        WindowScreenshot = 1,
        [DisplayLabel("Standalone")]
        Empty = 2,
        //[DisplayLabel("Background")]
        //BackgroundOnly = 3,
    }

    public enum Tool
    {
        [DisplayLabel("Bounding Box", 0)]
        BoundingBox = 0,
        [DisplayLabel("Ruler", 1)]
        Ruler = 1,
        [DisplayLabel("Color Picker", 2)]
        ColorPicker = 2,
    }

    public static class EnumExtensions
    {
        public static string GetDisplayLabel(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = (DisplayLabelAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayLabelAttribute), false);

            return attributes.Length > 0 ? attributes[0].Label : enumValue.ToString();
        }

        public static int GetOrder(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = (DisplayLabelAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayLabelAttribute), false);

            return attributes.Length > 0 ? attributes[0].Order : -1;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayLabelAttribute : Attribute
    {
        public string Label { get; private set; }
        public int Order { get; private set; } = -1;

        public DisplayLabelAttribute(string label, int order = -1)
        {
            Label = label;
            Order = order;
        }
    }
}
