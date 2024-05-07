using System.Diagnostics;

namespace PixelRuler.Models
{
    public class CommandTargetInfo
    {
        public CommandTargetInfo()
        {

        }

        public CommandTargetInfo(string displayName, string commandString, string commandArgs, bool enabled = true)
        {
            DisplayName = displayName;
            CommandExe = commandString;
            CommandArgs = commandArgs;
            Enabled = enabled;
        }

        public CommandTargetInfo Clone()
        {
            return (CommandTargetInfo)this.MemberwiseClone();
        }

        public bool Enabled { get; set; }

        public string? DisplayName { get; set; }

        public string? CommandExe { get; set; }

        public string CommandArgs { get; set; } = string.Empty;

        public string FullCommandString
        {
            get
            {
                return $"{CommandExe} {CommandArgs}";
            }
        }

        private string EvaluateArgs(string filename)
        {
            if (CommandArgs.Contains("{filename}"))
            {
                return CommandArgs.Replace("{filename}", $"{filename}");
            }
            return CommandArgs;
        }

        public void Execute(string filename)
        {
            var cmdArgs = this.EvaluateArgs(filename);
            Process.Start(CommandExe, cmdArgs);
        }
    }
}
