using System;
using System.Collections.Generic;

namespace PixelRuler.Models
{
    public class PathSaveInfoToken
    {
        public PathSaveInfoToken(PathTokenType pathToken, string tokenName, string formatSpecifierName, FormatSpecifierType formatSpec, string helperText, string defaultFormatString)
        {
            PathToken = pathToken;
            TokenName = tokenName;
            FormatSpecifierName = formatSpecifierName;
            FormatSpec = formatSpec;
            HelperText = helperText;
            DefaultFormatString = defaultFormatString;
        }

        public PathTokenType PathToken { get; set; }
        public string TokenName { get; set; }
        public string FormatSpecifierName { get; set; }
        public FormatSpecifierType FormatSpec { get; set; }
        public string HelperText { get; set; }
        public string DefaultFormatString { get; set; }

        public string DefaultInsert
        {
            get
            {
                if (!string.IsNullOrEmpty(DefaultFormatString))
                {
                    return $"{{{TokenName}:{DefaultFormatString}}}";
                }
                else
                {
                    return $"{{{TokenName}}}";

                }
            }
        }
    }
    // TODO: on hover over token show it inserted in grey at that location (note: requires RichTextBox)
    // TODO: UI columns first..

    public static class PathSaveInfoUtil
    {
        public static List<PathSaveInfoToken> AllTokens;
        static PathSaveInfoUtil()
        {
            AllTokens = new List<PathSaveInfoToken>()
            {
                new PathSaveInfoToken(PathTokenType.DateTime, "datetime", "datetime_format", FormatSpecifierType.DateTime, "Date and Time ex. {datetime:yyyy_MM_dd} -> 2024_03_30", "yyyy_MM_dd_HHmmss"),
                new PathSaveInfoToken(PathTokenType.Width, "width", "min_digits", FormatSpecifierType.Integer, "Screenshot Width ex. {width:4} -> 0800", "4"),
                new PathSaveInfoToken(PathTokenType.Height, "height", "min_digits", FormatSpecifierType.Integer, "Screenshot Height ex. {height:4} -> 0600", "4"),
                new PathSaveInfoToken(PathTokenType.UnixTimeSec, "unixtime_s", "", FormatSpecifierType.None, "Epoch Unix Timestamp in Seconds ex. {unixtime_s} -> 1711832151", ""),
                new PathSaveInfoToken(PathTokenType.UnixTimeMs, "unixtime_ms", "", FormatSpecifierType.None, "Epoch Unix Timestamp in Milliseconds ex. {unixtime_s} -> 1711832151543", ""),
                new PathSaveInfoToken(PathTokenType.RandomNum, "random_num", "length", FormatSpecifierType.Integer, "Random Number ex. {random_num:6} -> 849288", "8"),
                new PathSaveInfoToken(PathTokenType.RandomGuid, "random_guid", "", FormatSpecifierType.None, "Random GUID ex. {random_guid} -> b1b9261b-0ba5-4315-87f6-a00fd89565e7", ""),
                new PathSaveInfoToken(PathTokenType.WindowTitle, "window_title", "", FormatSpecifierType.None, "Window Title ex. {window_title} -> GitHub - Firefox", ""),
                new PathSaveInfoToken(PathTokenType.ProcessName, "process_name", "", FormatSpecifierType.None, "Process Name ex. {process_name} -> Firefox", ""),
                //new PathSaveInfoToken("local_iter", "min_digits", FormatSpecifierType.Integer, "Local Iterater for this destination ex. {local_iter:4} -> 0007", "4"),
                //new PathSaveInfoToken("global_iter", "min_digits", FormatSpecifierType.Integer, "Global Iterator for this program ex. {global_iter:4} -> 0014", "4"),
            };
        }

        public static string GetValue(PathTokenType pathToken, string formatString, ScreenshotInfo info)
        {
            switch (pathToken)
            {
                case PathTokenType.DateTime:
                    try
                    {
                        return info.DateTime.ToString(formatString);
                    }
                    catch (FormatException)
                    {
                        throw new PathEvaluationException("Invalid Format String for {datetime}");
                    }
                case PathTokenType.Width:
                    return info.Width.ToString($"D{formatString}");
                case PathTokenType.Height:
                    return info.Height.ToString($"D{formatString}");
                case PathTokenType.WindowTitle:
                    return info.WindowTitle?.ToString() ?? string.Empty;
                case PathTokenType.ProcessName:
                    return info.ProcessName?.ToString() ?? string.Empty;
                case PathTokenType.RandomGuid:
                    return Guid.NewGuid().ToString();
                case PathTokenType.RandomNum:
                    int numDigits = 10;
                    if (int.TryParse(formatString, out int result))
                    {
                        numDigits = result;
                    }
                    var max = Math.Pow(10, numDigits) - 1;
                    try
                    {
                        long randomNum = new Random().NextInt64((long)max);
                        return randomNum.ToString($"D{numDigits}");
                    }
                    catch
                    {
                        throw new PathEvaluationException("Invalid Format String for {random_num}: Out of Range");
                    }
                case PathTokenType.UnixTimeMs:
                    return (Math.Round((info.DateTime - DateTime.UnixEpoch).TotalMilliseconds)).ToString();
                case PathTokenType.UnixTimeSec:
                    return (Math.Round((info.DateTime - DateTime.UnixEpoch).TotalSeconds)).ToString();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum FormatSpecifierType
    {
        None = 0,
        DateTime = 1,
        Integer = 2,
    }

    public enum PathTokenType
    {
        DateTime = 0,
        Width = 1,
        Height = 2,
        UnixTimeSec = 3,
        UnixTimeMs = 4,
        RandomNum = 5,
        RandomGuid = 6,
        WindowTitle = 7,
        ProcessName = 8,
    }
}
