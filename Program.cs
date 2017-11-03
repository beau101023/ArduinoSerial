using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Ports;

namespace ArduinoSerial
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort arduinoPort = FindArduinoPort();

            if (arduinoPort == null)
            {
                Console.WriteLine("Error! Arduino not found.");
            }
            else
            {
                Console.WriteLine("Arduino found on port {0} running at {1} baud.", arduinoPort.PortName, arduinoPort.BaudRate);
            }


            arduinoPort.Write
        }

        static public SerialPort FindArduinoPort()
        {
            SerialPort currentPort = null;
            bool portFound = false;

            try
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    currentPort = new SerialPort(port, 9600);
                    if (DetectArduino(currentPort))
                    {
                        portFound = true;
                        break;
                    }
                    else
                    {
                        portFound = false;
                    }
                }
            }
            catch (Exception e)
            {
            }

            if (portFound == false)
            {
                return null;
            }
            else return currentPort;
        }

        public static void WriteInstruction(this SerialPort port, Opcode op, byte data1, byte data2, byte data3)
        {
            byte[] writeBytes = { (byte)op, data1, data2, data3 };

            if (!port.IsOpen)
            {
                port.Open();
            }

            port.Write(writeBytes, 0, writeBytes.Length);
        }

        static public bool DetectArduino(SerialPort currentPort)
        {
            // stores information to send to the arduino about what we're doing. byte 0 stores control information to tell it what to do with the remaining bytes
            byte[] infoPacket = new byte[4];

            // initialize a random buffer for authentication, arduino should return each byte divided by two and rounded down to the nearest whole number.
            Random r = new Random();
            byte[] randBuffer = new byte[4];
            r.NextBytes(randBuffer);

            infoPacket[0] = (byte)Opcode.Verification;
            // no more info needed
            infoPacket[1] = 0;
            infoPacket[3] = 0;
            infoPacket[4] = 0;

            currentPort.Open();
            currentPort.Write(infoPacket, 0, infoPacket.Length);
            currentPort.Write(randBuffer, 0, randBuffer.Length);

            // give the arduino time to respond (adjust, 50ms might be too short)
            Thread.Sleep(50);

            // if arduino has given 4 bytes, evaluate them to see if they are as expected
            if (currentPort.BytesToRead == 4)
            {
                byte[] readBuffer = new byte[4];

                currentPort.Read(readBuffer, 0, 4);

                for (int i = 0; i < 4; i++)
                {
                    if (readBuffer[i] != randBuffer[i] / 2)
                    {
                        // byte was not as expected
                        return false;
                    }
                }
                // all bytes evaluated correctly
                return true;
            }
            else // if the number of bytes does not equal 4, this likely means that the device isn't correct or the arduino is malfunctioning
            {
                return false;
            }
        }

        public enum Opcode
        {
            Verification = 0, // figure out if this is the arduino
            Transmission = 1, // transmit data to be displayed
            Callback = 2 // tell arduino to give us data (figure out what to use this for)
        }

        public enum DataType
        {
            LifeState = 0,
            RainTimer = 1,

        }
    }
}
