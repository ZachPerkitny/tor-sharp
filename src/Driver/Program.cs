using System;
using TorController;

namespace Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            Controller controller = new Controller();
            controller.Connect();
            controller.Authenticate();

            Console.ReadLine();
        }
    }
}
