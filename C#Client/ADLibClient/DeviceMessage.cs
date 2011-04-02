using System;
using System.Text;

// This could be cleaned up with some more OOP

namespace ADLibClient {
    class DeviceMessage {

        public static readonly int MAX_DEVICE = 255;
        public static readonly int MAX_DATATYPES = 3;
        public static readonly int MAX_TEXT_LEN = 255;
        public static readonly string DEFAULT_DELIMETER = "\v";     // Default delimeter character
        public static readonly String DEFAULT_CHAR_ENC = "ascii";   // Default character encoding

        // Target Controller information
        public string ipAddress { get; set; }
        public int port { get; set; }
        public string serialPort { get; set; }
        public bool isNetworkSend = true;

        // Packet Header definition
        public int deviceID { get; set; }           // ID Number of the device
        public int dataTypeID { get; set; }         // ID of the data type
        public double minValue { get; set; }
        public double maxValue { get; set; }
        public double currentValue { get; set; }
        public double delta { get; set; }
        public double normalizedValue { get; set; }
        public string text { get; set; }

        public DeviceMessage() { }

        // Numeric datatype constructor
        public DeviceMessage(string ipAddress, int port, int deviceID, int dataTypeID, double minValue, double maxValue, double currentValue, double delta) {
            create(ipAddress, port, deviceID, dataTypeID, minValue, maxValue, currentValue, delta);
        }

        public DeviceMessage(string serialPort, int deviceID, int dataTypeID, double minValue, double maxValue, double currentValue, double delta) {
            create(serialPort, deviceID, dataTypeID, minValue, maxValue, currentValue, delta);
        }

        // Text and Error datatype constructor
        public DeviceMessage(string ipAddress, int port, int deviceID, int dataTypeID, string text) {
            create(ipAddress, port, deviceID, dataTypeID, text);
        }

        public DeviceMessage(string serialPort, int deviceID, int dataTypeID, string text) {
            create(serialPort, deviceID, dataTypeID, text);
        }

        // Normailized datatype constructor
        public DeviceMessage(string ipAddress, int port, int deviceID, int dataTypeID, double normalizedValue) {
            create(ipAddress, port, deviceID, dataTypeID, normalizedValue);
        }

        public DeviceMessage(string serialPort, int deviceID, int dataTypeID, double normalizedValue) {
            create(serialPort, deviceID, dataTypeID, normalizedValue);
        }

        // Numeric data type
        public void create(string ipAddress, int port, int deviceID, int dataTypeID, double minValue, double maxValue, double currentValue, double delta) {

            this.ipAddress = ipAddress;
            this.port = port;
            this.deviceID = deviceID;
            this.dataTypeID = dataTypeID;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.currentValue = currentValue;
            this.delta = delta;
        }

        public void create(string serialPort, int deviceID, int dataTypeID, double minValue, double maxValue, double currentValue, double delta) {

            this.serialPort = serialPort;
            this.deviceID = deviceID;
            this.dataTypeID = dataTypeID;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.currentValue = currentValue;
            this.delta = delta;
            isNetworkSend = false;
        }

        // Text and Error data type
        public void create(string ipAddress, int port, int deviceID, int dataTypeID, string text) {
            this.ipAddress = ipAddress;
            this.port = port;
            this.deviceID = deviceID;
            this.dataTypeID = dataTypeID;
            this.text = text;
        }

        public void create(string serialPort, int deviceID, int dataTypeID, string text) {
            this.serialPort = serialPort;
            this.deviceID = deviceID;
            this.dataTypeID = dataTypeID;
            this.text = text;
            isNetworkSend = false;
        }

        // Normailized datatype
        public void create(string ipAddress, int port, int deviceID, int dataTypeID, double normalizedValue) {
            this.ipAddress = ipAddress;
            this.port = port;
            this.deviceID = deviceID;
            this.dataTypeID = dataTypeID;
            this.normalizedValue = normalizedValue;
        }

        public void create(string serialPort, int deviceID, int dataTypeID, double normalizedValue) {
            this.serialPort = serialPort;
            this.deviceID = deviceID;
            this.dataTypeID = dataTypeID;
            this.normalizedValue = normalizedValue;
            isNetworkSend = false;
        }

        public string ToFramedString() {
            string message;

            message = Convert.ToString(deviceID) + DEFAULT_DELIMETER + Convert.ToString(dataTypeID) + DEFAULT_DELIMETER;

            if (dataTypeID == 0 || dataTypeID == 1) {
                message += text;
            } else if (dataTypeID == 2) {
                message += Convert.ToString(minValue) + DEFAULT_DELIMETER + Convert.ToString(maxValue) + DEFAULT_DELIMETER;
                message += Convert.ToString(currentValue) + DEFAULT_DELIMETER + Convert.ToString(delta);
            } else if (dataTypeID == 3) {
                message += normalizedValue;
            }

            message += '\0';

            return message;
        }
        
        public override string ToString() {
            string message;

            message = "Device ID        : " + Convert.ToString(deviceID);

            if (dataTypeID == 1) {
                message = "\nData Type ID     : 0 (Error datatype)";
                message += "\nText             : " + text;
            } else if (dataTypeID == 1) {
                message = "\nData Type ID     : 1 (Text datatype)";
                message += "\nText             : " + text;
            } else if (dataTypeID == 2) {
                message = "\nData Type ID    : 2 (Numeric datatype)";
                message += "\nMin Value       : " + Convert.ToString(minValue);
                message += "\nMax Value       : " + Convert.ToString(maxValue);
                message += "\nCurrent  Value  : " + Convert.ToString(currentValue);
                message += "\nDelta           : " + Convert.ToString(delta);
            } else if (dataTypeID == 3) {
                message = "\nData Type ID     : 3 (Normalized datatype)";
                message += "\nValue             : " + Convert.ToString(normalizedValue); ;
            }

            return message;
        }

        public byte[] ToEncodedByteArray() {

            Encoding encoding = Encoding.GetEncoding(DEFAULT_CHAR_ENC); // Character encoding
            string message = ToFramedString();

            byte[] buf = encoding.GetBytes(message);
            return buf;
        }
    }
}