using System;
using System.Linq;
using System.Threading;
using XInput.Wrapper;

namespace GpTest.Con
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"XInput subsystem is {(X.Available ? "" : "not ")}available.");

            foreach (var gp in X.Gamepads)
                Console.WriteLine($"Gamepad #{gp.Index} {(gp.Connected ? "connected" : "disconnected")}");

            Console.WriteLine("\nCurrent connected:");
            foreach (var gp in X.AvailableGamepads)
                Console.WriteLine($"Gamepad #{gp.Index} is available");

            while (X.AvailableGamepads.Count() < 1) {
                Console.Write(".");
                Thread.Sleep(1000);
            }
            Console.WriteLine();

            X.Gamepad gpad = X.AvailableGamepads.First();

            Console.WriteLine($"[+] Gamepad #{gpad.Index} connected");

            do {
                bool changed = gpad.Update();
                if (changed) {
                }

                Thread.Sleep(100);
            } while (gpad.Available);
            Console.WriteLine();

            Console.WriteLine("\nPress [Enter] to exit...");
            Console.ReadLine();
        }
    }
}