using System;
using TorControl;
using TorControl.Enum;

namespace Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Commander commander = new Commander())
            {
                commander.Connect();
                commander.Authenticate("finance56");
                commander.SetEvents(new EventCode[] { EventCode.CIRC, EventCode.DEBUG, EventCode.NOTICE });
                commander.Signal(Signal.DEBUG);
                Console.ReadLine();
            }   

            Console.ReadLine();
        }
    }
}
