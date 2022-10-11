using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// adapted from https://github.com/drewarnett/pypicoboard/blob/main/picoboard.py

namespace PicoBoard
{
    class PicoBoard
    {
        SerialPort p;

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
            p.ReadTimeout = 100;
            p.BaudRate = 38400;
        }

        public bool Read()
        {
            byte[] buffer = new byte[2];
            bool success = true;
            Console.WriteLine("Reading 2 bytes");
            try
            {
                p.Read(buffer, 0, 2);
            } catch
            {
                success = false;
            }

            if (success)
            {

                byte upper = buffer[0];
                byte lower = buffer[1];
                if (((upper >> 7) & 1) == 1)
                {
                    Console.WriteLine("First assert passed");
                }
                else
                {
                    Console.WriteLine("First assert failed");
                }
            }
            return success;
        }

        public void Test()
        {
            p.Open();
            Read();
        }
    }
}
