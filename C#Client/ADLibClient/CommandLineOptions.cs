using CommandLine;
using System.Text;

namespace cs693 {
    class CommandLineOptions {

        [Option("ip", "IP Address", Required = true, HelpText = "IP address to send message to")]
        public string ipAddress = null;

        [Option("p", "port", Required = true, HelpText = "TCP Port to send message over")]
        public string port = null;

        [Option("d", "Device", Required = true, HelpText = "Device ID of target.")]
        public int device = -1;

        [Option("t", "Datatype", Required = true, HelpText = "The data type of the message.")]
        public int datatype = 0;

        [Option("min", "Minimum Value", Required = false, HelpText = "The minimum value for a numeric datatype message")]
        public double minimumValue = 0;

        [Option("max", "Maximum Value", Required = false, HelpText = "The maximum value for a numeric datatype message")]
        public double maximumValue = 0;

        [Option("c", "Current Value", Required = false, HelpText = "The current value for a numeric datatype message")]
        public double currentValue = 0;

        [Option("dv", "Delta Value", Required = false, HelpText = "The delta value for a numeric datatype message")]
        public double deltaValue = 0;

        [Option("m", "Message Text", Required = false, HelpText = "The text string for text datatype message.")]
        public string messageText = null;

        [HelpOption(HelpText = "Dispaly this help screen.")]
        public string GetUsage() {
            StringBuilder help = new StringBuilder();
            help.AppendLine("Guide Application Help Screen!");
            return help.ToString();
        }
    }
}
