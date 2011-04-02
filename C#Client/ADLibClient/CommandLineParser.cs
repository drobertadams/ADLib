using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace ADLibClient {
    class CommandLineParser {

        public DeviceMessage parse(string[] args) {

            string ipAddress = null;
            string serialPort = null;
            string text = null;
            int port = 0;
            int device = 0;
            int dataType = 0;
            double minValue = 0;
            double maxValue = 0;
            double currentValue = 0;
            double deltaValue = 0;
            double normalizedValue = 0;
            DeviceMessage message = null;
            bool isNetworkSend = true;

            // Build hash table of command line args
            Hashtable hashtable = new Hashtable();

            for (int x = 0; x < args.Length; x++) {

                int startOffset = 0;

                if (args[x].StartsWith("-") || args[x].StartsWith("/"))
                    startOffset = 1;

                if (args[x].StartsWith("--"))
                    startOffset = 2;

                args[x] = args[x].Substring(startOffset);

                try {
                    hashtable.Add(args[x].Substring(0, args[x].IndexOf('=')).ToLower(), args[x].Substring(args[x].IndexOf('=') + 1));
                } catch {
                    ShowHelp("Invalid command line.");
                }

            }

            // Validate all required options have been passed and are valid

            // IP Address
            if (hashtable.ContainsKey("address")) {
                
                ipAddress = (string)hashtable["address"];

                string validIpAddress = "^" + @"(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
                string validHostname = "^" + @"(([a-zA-Z]|[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z]|[A-Za-z][A-Za-z0-9\-]*[A-Za-z0-9])$";

                Regex validIpAddressRegex = new Regex(validIpAddress);
                Regex validHostnameRegex = new Regex(validHostname);

                if (!validIpAddressRegex.IsMatch(ipAddress))
                    if (!validHostnameRegex.IsMatch(ipAddress))
                        ShowHelp("Invalid IP Address or hostname");
                
                // TCP Port
                if (hashtable.ContainsKey("port") && IsInteger(hashtable["port"].ToString())) {

                    port = Convert.ToInt32(hashtable["port"]);

                    if (port < 1 || port > 65535)
                        ShowHelp("Invalid port entered. Valid options are 1 - 65,535.");
                } else
                    ShowHelp("Missing required TCP port parameter --port.");
                
                isNetworkSend = true;
            // Serial port
            } else if (hashtable.ContainsKey("serial")) {

                serialPort = (string)hashtable["serial"];

                string validSerialPort = "^" + @"com[1-9]|[1-9]$";
                Regex validSerailPortRegex = new Regex(validSerialPort, RegexOptions.IgnoreCase);

                if (!validSerailPortRegex.IsMatch(serialPort))
                    ShowHelp("Invalid serial port entered. Valid options are com1 - com9 or 1 - 9.");

                if (!serialPort.StartsWith("com"))
                    serialPort = "com" + serialPort;

                isNetworkSend = false;
            } else
                ShowHelp("Missing required --address or --serial parameter.");

            // Device ID
            if (hashtable.ContainsKey("device") && IsInteger(hashtable["device"].ToString())) {

                device = Convert.ToInt32(hashtable["device"]);

                if (device < 1 || device > DeviceMessage.MAX_DEVICE)
                    ShowHelp("Invalid device ID entered. Valid options are 1 - " + DeviceMessage.MAX_DEVICE);

            } else
                ShowHelp("Missing required device ID parameter --device.");

            // Data type
            if (hashtable.ContainsKey("type") && IsInteger(hashtable["type"].ToString())) {

                dataType = Convert.ToInt32(hashtable["type"]);

                if (dataType < 0 || dataType > DeviceMessage.MAX_DATATYPES)
                    ShowHelp("Invalid data type entered. Valid options are 0 - " + DeviceMessage.MAX_DATATYPES);
            } else
                ShowHelp("Missing required data type parameter --type.");

            // If we make it here, the required options are present and valid. Now we can validate specific options needed for the data type
            if (dataType == 0) { // Error data type
                if (!hashtable.ContainsKey("text"))
                    ShowHelp("A text string is required for an error datatype.");

                text = (string)hashtable["text"];

                message = new DeviceMessage(ipAddress, port, device, dataType, text);
            } else if (dataType == 1) { // Text data type
                if (!hashtable.ContainsKey("text"))
                    ShowHelp("A text string is required for a text datatype.");

                text = (string)hashtable["text"];

                message = new DeviceMessage(ipAddress, port, device, dataType, text);
            } else if (dataType == 2) { // Numeric data type

                if (!hashtable.ContainsKey("min"))
                    ShowHelp("Missing required minimum value parameter --min.");
                else {
                    if (!IsDecimal(hashtable["min"].ToString()))
                        ShowHelp("Invalid value for minimum value parameter --min.");
                
                    minValue = Convert.ToDouble(hashtable["min"]);
                }

                if (!hashtable.ContainsKey("max"))
                    ShowHelp("Missing required maximum value parameter --max.");
                else {
                    if (!IsDecimal(hashtable["max"].ToString()))
                        ShowHelp("Invalid value for maximum value parameter --max.");

                    maxValue = Convert.ToDouble(hashtable["max"]);
                }

                if (!hashtable.ContainsKey("current"))
                    ShowHelp("Missing required current value parameter --current.");
                else {
                    if (!IsDecimal(hashtable["current"].ToString()))
                        ShowHelp("Invalid value for current value parameter --current.");

                    currentValue = Convert.ToDouble(hashtable["current"]);
                }

                if (!hashtable.ContainsKey("delta"))
                    ShowHelp("Missing required delta value parameter --delta.");
                else {
                    if (!IsDecimal(hashtable["delta"].ToString()))
                        ShowHelp("Invalid value for delta value parameter --delta.");

                    deltaValue = Convert.ToDouble(hashtable["delta"]);
                }

                message = new DeviceMessage(ipAddress, port, device, dataType, minValue, maxValue, currentValue, deltaValue);
            } else if (dataType == 3) { // Normalized data type
                if (!hashtable.ContainsKey("value"))
                    ShowHelp("Missing required current value parameter --value.");
                else {
                    if (!IsDecimal(hashtable["value"].ToString()))
                        ShowHelp("Invalid value for current value parameter --value.");

                    normalizedValue = Convert.ToDouble(hashtable["value"]);

                    if (normalizedValue < 0 || normalizedValue > 100)
                        ShowHelp("Normalized data value must be between 0 and 100.");
                }

                if (isNetworkSend)
                    message = new DeviceMessage(ipAddress, port, device, dataType, normalizedValue);
                else
                    message = new DeviceMessage(serialPort, device, dataType, normalizedValue);
            }

            return message;
        }

        static void ShowHelp(String message) {
            Console.WriteLine("\n" + message + "\n");
            Console.WriteLine("Usage: Network");
            Console.WriteLine("       client --address=IPADDRESS --port=PORT --device=DEVICEID --type=0 --text=STRING");
            Console.WriteLine("       client --address=IPADDRESS --port=PORT --device=DEVICEID --type=1 --text=STRING");
            Console.WriteLine("       client --address=IPADDRESS --port=PORT --device=DEVICEID --type=2 --min=MINVALUE --max=MAXIMUMVALUE --current=CURRENTVALUE --delta=DELTAVALUE");
            Console.WriteLine("       client --address=IPADDRESS --port=PORT --device=DEVICEID --type=3 --value=NORMALIZEDVALUE");
            Console.WriteLine();
            Console.WriteLine("Usage: Serial");
            Console.WriteLine("       client --serial=COMPORT --device=DEVICEID --type=0 --text=STRING");
            Console.WriteLine("       client --serial=COMPORT --device=DEVICEID --type=1 --text=STRING");
            Console.WriteLine("       client --serial=COMPORT --device=DEVICEID --type=2 --min=MINVALUE --max=MAXIMUMVALUE --current=CURRENTVALUE --delta=DELTAVALUE");
            Console.WriteLine("       client --serial=COMPORT --port=PORT --device=DEVICEID --type=3 --value=NORMALIZEDVALUE");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("       None");

            Environment.Exit(-1);
        }

        public static bool IsDecimal(string value) {
            try {
                Convert.ToDouble(value);
                return true;
            }  catch {
                return false;
            }
        } //IsDecimal

        public static bool IsInteger(string value) {
            try {
                Convert.ToInt32(value);
                return true;
            }  catch  {
                return false;
            }
        } //IsInteger
    }
}