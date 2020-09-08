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

            X.Gamepad gamepad = X.AvailableGamepads.First();

            Console.WriteLine($"[+] Gamepad #{gamepad.Index} connected");

            Console.WriteLine("\nPress [Enter] to exit...");
            Console.ReadLine();
        }
    }
}