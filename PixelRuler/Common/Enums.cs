using System;

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

    public enum DefaultTool
    {
        [DisplayLabel("Last Selected", 0)]
        LastSelectedTool = -1,
        [DisplayLabel("Bounding Box", 1)]
        BoundingBox = Tool.BoundingBox,
        [DisplayLabel("Ruler", 2)]
        Ruler = Tool.Ruler,
        [DisplayLabel("Color Picker", 3)]
        ColorPicker = Tool.ColorPicker,
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

        public static bool IsLeft(this SizerEnum sizerEnum)
        {
            return sizerEnum == SizerEnum.TopLeft || sizerEnum == SizerEnum.BottomLeft || sizerEnum == SizerEnum.CenterLeft;
        }

        public static bool IsRight(this SizerEnum sizerEnum)
        {
            return sizerEnum == SizerEnum.TopRight || sizerEnum == SizerEnum.BottomRight || sizerEnum == SizerEnum.CenterRight;
        }

        public static bool IsBottom(this SizerEnum sizerEnum)
        {
            return sizerEnum == SizerEnum.BottomLeft || sizerEnum == SizerEnum.BottomRight || sizerEnum == SizerEnum.BottomCenter;
        }

        public static bool IsTop(this SizerEnum sizerEnum)
        {
            return sizerEnum == SizerEnum.TopLeft || sizerEnum == SizerEnum.TopRight || sizerEnum == SizerEnum.TopCenter;
        }

        public static SizerPosX GetXFlag(this SizerEnum sizerEnum)
        {
            if (sizerEnum.IsLeft())
            {
                return SizerPosX.Left;
            }
            else if (sizerEnum.IsRight())
            {
                return SizerPosX.Right;
            }
            else
            {
                return SizerPosX.Centered;
            }
        }

        public static SizerPosY GetYFlag(this SizerEnum sizerEnum)
        {
            if (sizerEnum.IsBottom())
            {
                return SizerPosY.Below;
            }
            else if (sizerEnum.IsTop())
            {
                return SizerPosY.Above;
            }
            else
            {
                return SizerPosY.Centered;
            }
        }
    }

    public enum SizerPosX
    {
        Centered = 0,
        Left = 1,
        Right = 2,
    }
    public enum SizerPosY
    {
        Centered = 0,
        Below = 1,
        Above = 2,
    }

    public enum SizerEnum
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3,
        CenterLeft = 4,
        CenterRight = 5,
        TopCenter = 6,
        BottomCenter = 7,
    }

    public enum ZoomBoxCase
    {
        None = 0,
        QuickZoom = 1,
        Resizer = 2,
        ColorPicker = 3,
        ScreenshotBoundSelection = 4,
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
