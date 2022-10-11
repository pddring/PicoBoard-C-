using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// adapted from Datasheet: https://twiki.cern.ch/twiki/pub/Sandbox/DaqSchoolExercise14/Picoboard_protocol.pdf

namespace PicoBoard
{
    class PicoBoard
    {
        SerialPort p;
        byte[] buffer = new byte[18];

        int[] values = new int[8];

        public void Connect()
        {
            p.Open();
        }

        public void Disconnect()
        {
            p.Close();
        }

        public enum SensorID
        {
            RESISTANCE_D = 0,
            RESISTANCE_C = 1,
            RESISTANCE_B = 2,
            BUTTON = 3,
            RESISTANCE_A = 4,
            LIGHT = 5,
            SOUND = 6,
            SLIDER = 7
        }

        public int GetSensor(SensorID sensor)
        {
            return values[(int)sensor];
        }

        public PicoBoard(string portName = "")
        {
            // detect portname
            if (portName == "")
            {
                foreach(string name in SerialPort.GetPortNames())
                {
                    Console.WriteLine($"Found port: {name}");
                    portName = name;
                    break;
                }
            }

            // connect to the serial port
            p = new SerialPort(portName);

            // whole 18 bytes should arrive within 7.2ms
            p.ReadTimeout = 10; 
            p.BaudRate = 38400;
        }

        public void RequestData()
        {
            // send request (send 0x01)
            buffer[0] = 1;
            p.Write(buffer, 0, 1);
        }

        public bool Read()
        {
            bool success = true;
            int bytesRead = 0;
            
            try
            {
                //Console.WriteLine("Reading 18 byte packet");

                // seems to work better requesting 1 byte first then all remaining bytes afterwards
                bytesRead = p.Read(buffer, 0, 1);
                bytesRead += p.Read(buffer, 1, 17);
                //Console.WriteLine($"Read {bytesRead} byte(s)");
                if(bytesRead != 18)
                {
                    success = false;
                }
            }
            catch
            {
                success = false;
            }

            if (success)
            {
                for (int i = 0; i < 18; i += 2)
                {
                    byte upper = buffer[i];
                    byte lower = buffer[i+1];

                    // sanity check
                    if (!(((upper >> 7) & 1) == 1))
                        throw new Exception($"Invalid data detected: {upper} {lower} ");
                    if (!(((lower >> 7) & 1) == 0))
                        throw new Exception($"Invalid data detected: {upper} {lower} ");

                    // process data from pair of bytes
                    int channel = (upper >> 3) & 0xf;
                    int value = (upper & 0x7) << 7 | (lower & 0x7f << 0);

                    // check firmware
                    if(channel == 15)
                    {
                        if (value != 4) { 
                            throw new Exception("Invalid firmware value");
                        }
                    }
                    
                    // store channel value
                    if (channel < 8)
                    {
                        values[channel] = value;
                    }
                }

            }
            return success;
        }

        public bool Update()
        {
            RequestData();
            return Read();
        }

        public void ConsoleOutput()
        {
            Console.Write($"Resistance: {GetSensor(SensorID.RESISTANCE_A)}");
            Console.Write($" {GetSensor(SensorID.RESISTANCE_B)}");
            Console.Write($" { GetSensor(SensorID.RESISTANCE_C)}");
            Console.WriteLine($" { GetSensor(SensorID.RESISTANCE_D)}");
            Console.Write($"Button: {GetSensor(SensorID.BUTTON)}");
            Console.Write($" Light: {GetSensor(SensorID.LIGHT)}");
            Console.Write($" Sound: {GetSensor(SensorID.SOUND)}");
            Console.WriteLine($" Slider: {GetSensor(SensorID.SLIDER)}");
        }

        public void Test()
        {
            p.Open();
            RequestData();
            if (Read())
            {
                ConsoleOutput();
            }
            p.Close();
        }
    }
}
