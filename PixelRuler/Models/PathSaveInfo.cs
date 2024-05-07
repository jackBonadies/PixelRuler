using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Wpf.Ui.Controls;

namespace PixelRuler.Models
{
    public class PathSaveInfo
    {
        public PathSaveInfo()
        {

        }

        public PathSaveInfo(string displayName, string baseDirectory, string filePattern, string extension, bool enabled = true)
        {
            DisplayName = displayName;
            BaseDirectory = baseDirectory;
            FilePattern = filePattern;
            Extension = extension;
            Enabled = enabled;
        }

        public PathSaveInfo Clone()
        {
            return (PathSaveInfo)this.MemberwiseClone();
        }

        public bool Enabled { get; set; }

        public bool IsDefault { get; set; }

        public string? DisplayName { get; set; }

        public string? BaseDirectory { get; set; }

        public string? FilePattern { get; set; }

        public string? Extension { get; set; }

        public SymbolRegular Icon { get; set; } = SymbolRegular.Save24;

        public string Evaluate(
            ScreenshotInfo info,
            bool withDirectory = false,
            bool withExtension = false)
        {
            string filePatternEvaluated = FilePattern.ToString();
            var matches = tokenRegex.Matches(filePatternEvaluated);
            foreach (Match m in matches.Reverse())
            {
                var capture = m.Captures[0];
                var capturedValue = capture.Value;
                var colonIndex = capturedValue.IndexOf(':');
                string tokenString;
                string formatString = string.Empty;
                if (colonIndex == -1)
                {
                    tokenString = capturedValue.Substring(1, capturedValue.Length - 2);
                }
                else
                {
                    tokenString = capturedValue.Substring(1, colonIndex - 1);
                    formatString = capturedValue.Substring(colonIndex + 1, capturedValue.LastIndexOf('}') - colonIndex - 1);
                }
                var tokenInfo = PathSaveInfoUtil.AllTokens.Where(it => it.TokenName == tokenString).FirstOrDefault();

                //if(Enum.TryParse(typeof(PathTokenType), capturedValue, out object? tokenType))
                if (tokenInfo == null)
                {
                    throw new PathEvaluationException($"Unknown Token: {tokenString}");
                }
                else
                {
                    string evalValue = PathSaveInfoUtil.GetValue(tokenInfo.PathToken, formatString, info);
                    var startIndex = capture.Index;
                    var length = capture.Length;
                    filePatternEvaluated = filePatternEvaluated.Substring(0, startIndex) + filePatternEvaluated.Substring(startIndex + length);
                    filePatternEvaluated = filePatternEvaluated.Insert(startIndex, evalValue);
                }

            }

            if (withDirectory)
            {
                var baseDirEval = Environment.ExpandEnvironmentVariables(BaseDirectory);
                filePatternEvaluated = System.IO.Path.Combine(baseDirEval, filePatternEvaluated);
            }

            if (withExtension)
            {
                filePatternEvaluated = $"{filePatternEvaluated}.{Extension}";
            }

            return filePatternEvaluated;
        }

        public string SaveImage(Bitmap bmp, ScreenshotInfo info)
        {
            string fname = this.Evaluate(info, true, true);
            ImageCommon.SaveImage(fname, bmp);
            info.LastSavedPath = fname;
            return fname;
        }

        private static readonly Regex tokenRegex = new Regex(@"\{([^}:]+)(?::(.*?))?\}", RegexOptions.Compiled);


    }

    public class PathEvaluationException : Exception
    {
        public PathEvaluationException(string message) : base(message)
        {
        }

        public int startIndex = -1;
        public int length = -1;
    }
}
