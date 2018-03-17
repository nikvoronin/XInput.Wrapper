// XInput.Wrapper by Nikolai Voronin
// http://github.com/nikvoronin/xinput.wrapper
// Version 0.4 (July 15, 2017)
// Under the MIT License (MIT)
//

using System;
using System.Threading;

namespace XInput.Wrapper
{
    public static partial class X
    {
        static Thread updateThread;
        static SynchronizationContext uiContext;
        static CancellationTokenSource cts;

        public static int UpdatesPerSecond = 30;

        public static readonly Gamepad Gamepad1 = new Gamepad(0);
        public static readonly Gamepad Gamepad2 = new Gamepad(1);
        public static readonly Gamepad Gamepad3 = new Gamepad(2);
        public static readonly Gamepad Gamepad4 = new Gamepad(3);
        static X() { }

        /// <summary>
        /// Tests availability of the XInput_1.4 subsystem. 
        /// This one should not call often! It is not cached.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                bool isAvailable = false;

                try
                {
                    Native.XINPUT_STATE state = new Native.XINPUT_STATE();
                    Native.XInputGetState(0, ref state);
                    isAvailable = true;
                }
                catch
                {
                    isAvailable = false;
                }

                return isAvailable;
            }
        } // IsAvailable

        private static void OnEvent(object sender, EventHandler handler)
        {
            EventHandler pceh = handler;

            if (uiContext != null)
                uiContext.Post((o) => pceh?.Invoke(sender, EventArgs.Empty), null);
            else
                pceh?.Invoke(sender, EventArgs.Empty);
        }

        private static void OnKeyEvent(object sender, EventHandler<KeyEventArgs> handler, Gamepad.ButtonFlags buttons)
        {
            EventHandler<KeyEventArgs> pceh = handler;

            if (uiContext != null)
                uiContext.Post((o) => pceh?.Invoke(sender, new KeyEventArgs(buttons)), null);
            else
                pceh?.Invoke(sender, new KeyEventArgs(buttons));
        }

        public class KeyEventArgs : EventArgs
        {
            public Gamepad.ButtonFlags Buttons = Gamepad.ButtonFlags.None;
            public KeyEventArgs() { }
            public KeyEventArgs(Gamepad.ButtonFlags buttons) { Buttons = buttons; }
        }
    } // class X
}