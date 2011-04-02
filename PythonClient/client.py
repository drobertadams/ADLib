#!/usr/bin/env python
# encoding: utf-8
"""
client.py

Client that sends data to an Arduino ambient display device. Currently only
uses the serial connection.

Created by D. Robert Adams on 2011-02-24.

Known issues:
    * Sending anything via the serial port to the Arduino will reset
      it. For the Uno, you can connect a 100uF capacitor between GND and RST (short
      lead into GND) to disable auto-reset.
"""

import optparse
import serial
import sys
import time

# Message data types.
ERROR = 0
TEXT = 1
NUMERIC = 2
NORMALIZED = 3

class ADLib:
    """ADLib Python Client Class.
    Usage: 
        adlib = ADLib("/dev/tty.usbmodem411", 9600)     # Init ADLib
        adlib.sendNormalized(device=1, value=45)        # Send data
    """
    
    def __init__(self, serial, baud):
        self.serialPort = serial
        self.baud = baud
    
    def _createMessage(device, dataType, \
        minimumValue=0, maximumValue=0, currentValue=0, deltaValue=0, \
        message="", normalizedValue=0):
        """Creates and returns a ADLib protocol string."""
        
        DELIMITER = '\v'
        
        msg = "%d%c%d%c" % (device, DELIMITER, dataType, DELIMITER)
        
        if dataType == NUMERIC:
            msg = msg + "%f%c%f%c%f%c%f" % \
                (minimumValue, DELIMITER, maximumValue, DELIMITER, currentValue, DELIMITER, deltaValue)
        elif dataType == TEXT or dataType == ERROR:
            msg = msg + message
        elif dataType == NORMALIZED:
            msg = msg + "%d" % normalizedValue
        
        # Add a \0 to the end for serial communication to tell the arduino
        # the end of the message.
        return "%s%c" % (msg, 0x00)
    
    def sendError(self, device, message):
        msg = self._createMessage(device, ERROR, message=message)
        self._writeSerial(msg)
    
    def sendText(self, device, message):
        msg = self._createMessage(device, TEXT, message=message)
        self._writeSerial(msg)
    
    def sendNumeric(self, device, min, max, current, delta):
        msg = self._createMessage(device, NUMERIC, minimumValue=min, maximumValue=max, currentValue=current, deltaValue=delta)
        self._writeSerial(msg)
    
    def sendNormalized(self, device, value):
        msg = self._createMessage(device, NORMALIZED, normalizedValue=value)
        self._writeSerial(msg)
    
    def _writeSerial(self, message):
        ser = serial.Serial(self.serialPort, self.baud, timeout=1)
        time.sleep(2)   # delay to allow the Arduino to reset, if necessary
        ser.write(message)        


#=========================================================================
# Handles the parsing of command-line arguments
#
def parseArgs():
    parser = optparse.OptionParser()
    parser.add_option("--serial",
                      action="store", type="string", dest="serial",
                      help="(required) serial device connected to the Arduino")
    parser.add_option("--device",
                      action="store", type="int", dest="device",
                      help="(required) device id of the target")
    parser.add_option("--type",
                      action="store", type="int", dest="dataType",
                      help="(required) datatype of the message [%d=error, %d=text, %d=numeric, %d=normalized]" % (ERROR, TEXT, NUMERIC, NORMALIZED))
    parser.add_option("--min",
                      action="store", type="float", dest="minimumValue",
                      help="minimum value for the numeric data type")
    parser.add_option("--max",
                      action="store", type="float", dest="maximumValue",
                      help="maximum value for the numeric data type")
    parser.add_option("--current",
                      action="store", type="float", dest="currentValue",
                      help="current value for the numeric data type")
    parser.add_option("--delta",
                      action="store", type="float", dest="deltaValue",
                      help="delta value for the numeric data type")
    parser.add_option("--text",
                      action="store", type="string", dest="message",
                      help="text message for the error and text data types")
    parser.add_option("--value",
                      action="store", type="int", dest="normalizedValue",
                      help="text string for the normalized data type")
    (options, args) = parser.parse_args()
    return options


#=========================================================================
# Validated command-line arguments.
#
def validateOptions(options):
    err = False
    
    # serial is required
    if not options.serial:
        sys.stderr.write('Serial device is required.\n')
        err = True
    
    # device is required
    if not options.device:
        sys.stderr.write('Device ID is required.\n')
        err = True
    
    # dataType is required and must be one of the recognized types.
    if options.dataType is None or options.dataType < ERROR or options.dataType > NORMALIZED:
        sys.stderr.write('Data type is required.\n')
        err = True
    
    # If the dataType is numeric, min, max, current, and delta are required.
    if options.dataType == NUMERIC:
        if options.minimumValue is None or options.maximumValue is None or options.currentValue is None or options.deltaValue is None:
            sys.stderr.write('Numeric data type requires minimum, maximum, current, and delta values.\n')
            err = True
    
    # If the dataType is text, message is required.
    if options.dataType == TEXT or options.dataType == ERROR:
        if options.message is None:
            sys.stderr.write('Text and Error data types require a message.\n')
            err = True
    
    # If the dataType is normalized, a normalized value is required.
    if options.dataType == NORMALIZED:
        if options.normalizedValue is None:
            sys.stderr.write('Normalized data type requires a normalized value.\n')
            err = True
    
    # If there was an error, exit.
    if err:
        sys.exit(1)


#=========================================================================
# Program entry.
#
def main(argv=None):
    # Handle the command-line options.
    options = parseArgs()
    validateOptions(options)
    
    al = ADLib(options.serial, 9600)
    
    if options.dataType == ERROR:
        al.sendError(options.device, options.message)
    elif options.datatype == TEXT:
        al.sendText(options.device, options.message)
    elif options.datatype == NUMERIC:
        al.sendNumeric(options.device, options.minimumValue. options.maximumValue, options.currentValue, options.deltaValue)
    elif options.datatype == NORMALIZED:
        al.sendNormalized(options.device, options.normalizedValue)
    else:
        sys.stderr.write('Invalid type.\n')
        sys.exit(1)

if __name__ == "__main__":
	main()