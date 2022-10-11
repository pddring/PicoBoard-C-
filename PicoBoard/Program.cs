using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoBoard
{
    class Program
    {
        static void Main(string[] args)
        {
            PicoBoard pb = new PicoBoard();
            pb.Test();
        }
    }
}
