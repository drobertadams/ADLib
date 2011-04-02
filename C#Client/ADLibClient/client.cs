using System;
using System.Net.Sockets;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ADLibClient {
    class Client {

        static void Main(string[] args) {

            DeviceMessage message = new DeviceMessage();

            CommandLineParser parser = new CommandLineParser();
            message = parser.parse(args);

            Console.WriteLine("Sending Text-Encoded Message (" + message.ToFramedString().Length + " bytes): ");
            Console.WriteLine(message.ToFramedString());

            if (message.isNetworkSend) {
                try {
                    // Create socket that is connected to server on specified port
                    TcpClient client = new TcpClient(message.ipAddress, message.port);
                    NetworkStream netStream = client.GetStream();

                    netStream.Write(message.ToEncodedByteArray(), 0, message.ToFramedString().Length);

                    netStream.Close();
                    client.Close();
                } catch (SocketException e) {
                    Console.WriteLine("Error: {0}", e.Message);
                } catch (Exception ex) {
                    Console.WriteLine("Error: {0}", ex.Message);
                }
            } else {
                try {
                    SerialPort comPort = new SerialPort(message.serialPort, 9600, Parity.None, 8, StopBits.One);

                    comPort.DtrEnable = true;
                    comPort.Open();
                    comPort.Write(message.ToEncodedByteArray(), 0, message.ToEncodedByteArray().Length);
                    comPort.Close();
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}