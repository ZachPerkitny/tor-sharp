using System;
using TorController;

namespace Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Controller controller = new Controller())
            {
                controller.Connect();
                controller.Authenticate("finance56");
            }   

            Console.ReadLine();
        }
    }
}
