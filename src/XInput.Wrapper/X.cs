﻿// XInput.Wrapper by Nikolai Voronin
// http://github.com/nikvoronin/xinput.wrapper
// Version 0.4 (June 27, 2017)
// Under the MIT License (MIT)
//
// Stick = Thumb
// Bumper = Shoulder
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace XInput.Wrapper
{
    public static class X
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
        /// Should not call often! This one not cached.
        /// </summary>
        public static bool IsAvailable
        {
            get {
                bool isAvailable = false;
                try {
                    Native.XINPUT_STATE state = new Native.XINPUT_STATE();
                    Native.XInputGetState(0, ref state);
                    isAvailable = true;
                }
                catch {
                    isAvailable = false;
                }

                return isAvailable;
            }
        }

        public sealed class Gamepad
        {
            public readonly uint Index;

            public readonly Battery GamepadBattery;
            public readonly Battery HeadsetBattery;

            Native.XINPUT_STATE state = new Native.XINPUT_STATE();

            uint packetNumber = 0;
            public uint PacketNumber { get { return state.dwPacketNumber; } }

            public Button A;
            public ButtonFlags ButtonsState = ButtonFlags.None;
            internal List<Button> Buttons = new List<Button>();

            internal Gamepad(uint index)
            {
                Index = index;
                GamepadBattery = new Battery(index, Battery.At.Gamepad);
                HeadsetBattery = new Battery(index, Battery.At.Headset);

                // UNDONE others buttons
                A = new Button(ButtonFlags.A);
                // LATER Add supported buttons only. should based on capabilities
                Buttons.Add(A);
            }

            bool isConnected;
            public bool IsConnected { get { return isConnected; }}

            public bool Enable { set { Native.XInputEnable(value); } }

            /// <summary>
            /// Update gamepad data
            /// </summary>
            /// <returns>TRUE - if state has been changed (button pressed, gamepad dis|connected, etc)</returns>
            public bool Update()
            {
                bool isChanged = false;

                //lastButtonsState = state.Current.Buttons;
                uint lastPacketNumber = state.dwPacketNumber;
                uint result = Native.XInputGetState(Index, ref state);

                packetNumber = state.dwPacketNumber;

                if (isConnected != (result == 0))
                {
                    isChanged = true;
                    isConnected = (result == 0);
                    if(ConnectionChanged != null)
                        OnConnectionChanged();
                }

                // UNDONE do not update often. Should update only for wireless. ?shall I add an update interval
                //if (isConnected)
                //    UpdateBattery();

                if (lastPacketNumber != packetNumber)
                {
                    isChanged = true;
                    if (StateChanged != null)
                        OnStateChanged();
                }

                // UNDONE  make list of axises  and update it
                if (isConnected && isChanged)
                {
                    ButtonsState = (ButtonFlags)state.Gamepad.wButtons;

                    ButtonFlags downButtons = ButtonFlags.None;
                    ButtonFlags upButtons = ButtonFlags.None;

                    foreach(Button tt in Buttons)
                    {
                        Button.Went wh = tt.Update(ref state);
                        if (wh == Button.Went.Down)
                            downButtons |= tt.Mask;
                        else
                            if (wh == Button.Went.Up)
                                upButtons |= tt.Mask;
                    }

                    if (downButtons != ButtonFlags.None)
                        OnKeyDown(downButtons);

                    if (upButtons != ButtonFlags.None)
                        OnKeyUp(upButtons);
                }

                // UNDONE KeyDown should send regular 
                //if ((Buttons != 0) && (KeyDown != null))
                //    OnKeyDown();

                // UNDONE Force feedback
                //DateTime now = DateTime.UtcNow;
                //if ((ffbL_IsActive && (now >= ffbL_StopTime)) &&
                //    (ffbR_IsActive && (now >= ffbR_StopTime)))
                //{
                //    StopVibrate();
                //}
                //else
                //{
                //    if (ffbL_IsActive && (now >= ffbL_StopTime))
                //        StopVibrateLLow();
                //    if (ffbR_IsActive && (now >= ffbR_StopTime))
                //        StopVibrateRHi();
                //}

                return isChanged;
            } // Update()

            #region // Events ////////////////////////////////////////////////////////////////////


            public event EventHandler ConnectionChanged;
            public event EventHandler StateChanged;

            public void OnStateChanged()
            {
                OnEvent(StateChanged);
            }

            public void OnConnectionChanged()
            {
                OnEvent(ConnectionChanged);
            }

            private void OnEvent(EventHandler handler)
            {
                EventHandler pceh = handler;

                if (uiContext != null)
                    uiContext.Post((o) => { pceh?.Invoke(this, EventArgs.Empty); }, null);
                else
                    pceh?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler<KeyEventArgs> KeyDown;
            public event EventHandler<KeyEventArgs> KeyUp;

            public void OnKeyDown(ButtonFlags buttons)
            {
                OnKeyEvent(KeyDown, buttons);
            }

            public void OnKeyUp(ButtonFlags buttons)
            {
                OnKeyEvent(KeyUp, buttons);
            }

            private void OnKeyEvent(EventHandler<KeyEventArgs> handler, ButtonFlags buttons)
            {
                EventHandler<KeyEventArgs> pceh = handler;

                if (uiContext != null)
                    uiContext.Post((o) => { pceh?.Invoke(this, new KeyEventArgs(buttons)); }, null);
                else
                    pceh?.Invoke(this, new KeyEventArgs(buttons)); ;
            }

            public class KeyEventArgs : EventArgs
            {
                public ButtonFlags Buttons = ButtonFlags.None;

                public KeyEventArgs() { }
                public KeyEventArgs(ButtonFlags buttons) { Buttons = buttons; }
            }
            #endregion

            public class Button
            {
                public readonly ButtonFlags Mask;
                public readonly string Name = string.Empty;
                public bool Supported { get; internal set; } // LATER fill in the flag based on the capabilities

                public bool Pressed;

                internal Button(ButtonFlags mask)
                {
                    Mask = mask;

                    if (Names.ContainsKey(mask))
                        Name = Names[mask];
                }

                /// <summary>
                /// Updates button state
                /// </summary>
                /// <param name="state">Gamepad global state</param>
                /// <returns>TRUE - button state was changed</returns>
                internal Went Update(ref Native.XINPUT_STATE state)
                {
                    bool hasFlag = ((ButtonFlags)state.Gamepad.wButtons).HasFlag(Mask);
                    bool stateChanged = hasFlag != Pressed;
                    Went lastAct = Went.None;

                    if (stateChanged)
                    {
                        // Do not call if we don't have any subscribers
                        if ((KeyDown != null) && hasFlag && !Pressed)
                        {
                            OnKeyDown();
                            lastAct = Went.Down;
                        }
                        else
                        {
                            if ((KeyUp != null) && Pressed && !hasFlag)
                            {
                                OnKeyUp();
                                lastAct = Went.Down;
                            }
                        }

                        Pressed = hasFlag && !Pressed;
                    } // if stateChanged

                    return lastAct;
                } // Update

                public event EventHandler KeyDown;
                protected virtual void __OnKeyDown(object o)
                {
                    EventHandler pceh = KeyDown;
                    pceh?.Invoke(this, EventArgs.Empty);
                }

                public void OnKeyDown()
                {
                    if (uiContext != null)
                        uiContext.Post(__OnKeyDown, this);
                    else
                        __OnKeyDown(this);
                }

                public event EventHandler KeyUp;
                protected virtual void __OnKeyUp(object o)
                {
                    EventHandler pceh = KeyUp;
                    pceh?.Invoke(this, EventArgs.Empty);
                }

                public void OnKeyUp()
                {
                    if (uiContext != null)
                        uiContext.Post(__OnKeyUp, this);
                    else
                        __OnKeyUp(this);
                }

                public readonly Dictionary<ButtonFlags, string> Names = new Dictionary<ButtonFlags, string>() {
                    { ButtonFlags.Up, "Dpad_Up" },
                    { ButtonFlags.Down, "Dpad_Down" },
                    { ButtonFlags.Left, "Dpad_Left" },
                    { ButtonFlags.Right, "Dpad_Right" },
                    { ButtonFlags.Start, "Start" },
                    { ButtonFlags.Back, "Back" },
                    { ButtonFlags.LStick, "Left_Stick" },
                    { ButtonFlags.RStick, "Right_Stick" },
                    { ButtonFlags.LBumper, "Left_Bumper" },
                    { ButtonFlags.RBumper, "Right_Bumper" },
                    { ButtonFlags.A, "Button_A" },
                    { ButtonFlags.B, "Button_B" },
                    { ButtonFlags.X, "Button_X" },
                    { ButtonFlags.Y, "Button_Y" }
                };

                internal enum Went { None, Down, Up }
            } // class Button

            [Flags]
            public enum ButtonFlags : uint
            {
                None = 0x0000,
                Up = 0x0001,
                Down = 0x0002,
                Left = 0x0004,
                Right = 0x0008,
                Start = 0x0010,
                Back = 0x0020,
                LStick = 0x0040,
                RStick = 0x0080,
                LBumper = 0x0100,
                RBumper = 0x0200,
                A = 0x1000,
                B = 0x2000,
                X = 0x4000,
                Y = 0x8000,
            };


            // LATER For binary state controls, such as digital buttons, the corresponding bit reflects whether or not the control is supported by the device. For proportional controls, such as thumbsticks, the value indicates the resolution for that control. Some number of the least significant bits may not be set, indicating that the control does not provide resolution to that level.
            public class Capability
            {
                uint uindex;
                Native.XINPUT_CAPABILITIES caps;

                internal Capability(uint userIndex)
                {
                    uindex = userIndex;
                }

                /// <summary>
                /// Update capabilities. Done automatically when controller is connected.
                /// </summary>
                /// <returns>TRUE - if updated successfully</returns>
                public bool Update()
                {
                    return
                        Native.XInputGetCapabilities(
                            uindex,
                            0x00000001, // always GAMEPAD_FLAG,
                            ref caps) == 0;
                }

                public SubType PadType { get { return (SubType)caps.SubType; } }

                public bool IsWireless { get { return ((Flags)caps.Flags).HasFlag(Flags.Wireless); } }
                public bool IsForceFeedback { get { return ((Flags)caps.Flags).HasFlag(Flags.ForceFeedback); } }
                public bool IsVoiceSupport { get { return ((Flags)caps.Flags).HasFlag(Flags.VoiceSupport); } }
                public bool IsNoNavigation { get { return ((Flags)caps.Flags).HasFlag(Flags.NoNavigation); } }
                public bool IsPluginModules { get { return ((Flags)caps.Flags).HasFlag(Flags.PMD_Supported); } }

                [Flags]
                public enum Flags : ushort
                {
                    VoiceSupport  = 0x0004,

                    //Windows 8 or higher only
                    ForceFeedback = 0x0001,   // Device supports force feedback functionality.
                    Wireless      = 0x0002,
                    PMD_Supported = 0x0008,   // Device supports plug-in modules.
                    NoNavigation  = 0x0010,   // Device lacks menu navigation buttons (START, BACK, DPAD).
                };

                public enum SubType : byte
                {
                    Unknown = 0x00,
                    Gamepad = 0x01,
                    Wheel = 0x02,
                    ArcadeStick = 0x03,
                    FlightStick = 0x04,
                    DancePad = 0x05,
                    Guitar = 0x06,
                    GuitarAlternate = 0x07,
                    DrumKit = 0x08,
                    GuitarBass = 0x0B,
                    ArcadePad = 0x13
                };
            } // class Capability

            public class Battery
            {
                uint uindex;
                Native.XINPUT_BATTERY_INFORMATION state;

                public readonly At Location;
                public SourceType Type { get { return (SourceType)state.BatteryType; }}
                public ChargeLevel Level { get { return (ChargeLevel)state.BatteryLevel; } }

                internal Battery(uint userIndex, At at)
                {
                    uindex = userIndex;
                    Location = at;
                }

                /// <summary>
                /// Update battery state
                /// </summary>
                /// <returns>TRUE - if updated successfully</returns>
                public bool Update() {
                    return Native.XInputGetBatteryInformation(uindex, (byte)Location, ref state) == 0;
                }

                public enum SourceType : byte
                {
                    Disconnected    = 0x00,
                    WiredNoBattery  = 0x01,
                    Alkaline        = 0x02,
                    NiMh            = 0x03,
                    Unknown         = 0xFF, 
                };

                // These are only valid for wireless, connected devices, with known battery types
                // The amount of use time remaining depends on the type of device.
                public enum ChargeLevel : byte
                {
                    Empty   = 0x00,
                    Low     = 0x01,
                    Medium  = 0x02,
                    Full    = 0x03
                };

                public enum At : byte
                {
                    Gamepad = 0x00,
                    Headset = 0x01,
                }
            } // class Battery
        } // class Gamepad

        public class Native
        {
            [DllImport("xinput1_4.dll")]
            public static extern uint XInputGetState
            (
                uint dwUserIndex,
                ref XINPUT_STATE pState
            );

            [DllImport("xinput1_4.dll")]
            public static extern uint XInputSetState
            (
                uint dwUserIndex,
                ref XINPUT_VIBRATION pVibration
            );

            [DllImport("xinput1_4.dll")]
            public static extern uint XInputGetCapabilities
            (
                uint dwUserIndex,
                uint dwFlags,
                ref XINPUT_CAPABILITIES pCapabilities
            );


            [DllImport("xinput1_4.dll")]
            public static extern uint XInputGetBatteryInformation
            (
                uint dwUserIndex,
                byte devType,
                ref XINPUT_BATTERY_INFORMATION pBatteryInformation
            );

            [DllImport("xinput1_4.dll")]
            public static extern uint XInputGetKeystroke
            (
                uint dwUserIndex,
                uint dwReserved,
                ref XINPUT_KEYSTROKE pKeystroke
            );

            [DllImport("xinput1_4.dll")]
            public static extern void XInputEnable
            (
                bool enable
            );

            [DllImport("xinput1_4.dll)")]
            public static extern uint XInputGetAudioDeviceIds
            (
                uint dwUserIndex,
                [MarshalAs(UnmanagedType.LPWStr)]out string pRenderDeviceId,
                ref uint pRenderCount,
                [MarshalAs(UnmanagedType.LPWStr)]out string pCaptureDeviceId,
                ref uint pCaptureCount
            );

            // ex StatePacket
            [StructLayout(LayoutKind.Explicit)]
            public struct XINPUT_STATE
            {
                [FieldOffset(0)]
                public uint dwPacketNumber;

                [FieldOffset(4)]
                public XINPUT_GAMEPAD Gamepad;
            }

            // ex PadState
            [StructLayout(LayoutKind.Explicit)]
            public struct XINPUT_GAMEPAD
            {
                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(0)]
                public ushort wButtons;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(2)]
                public byte bLeftTrigger;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(3)]
                public byte bRightTrigger;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(4)]
                public short sThumbLX;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(6)]
                public short sThumbLY;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(8)]
                public short sThumbRX;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(10)]
                public short sThumbRY;
            }

            // ex VibrationSpeed
            [StructLayout(LayoutKind.Sequential)]
            public struct XINPUT_VIBRATION
            {
                [MarshalAs(UnmanagedType.I2)]
                public ushort wLeftMotorSpeed;

                [MarshalAs(UnmanagedType.I2)]
                public ushort wRightMotorSpeed;
            }

            // ex DeviceCapability
            [StructLayout(LayoutKind.Explicit)]
            public struct XINPUT_CAPABILITIES
            {
                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(0)]
                public byte Type;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(1)]
                public byte SubType;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(2)]
                public ushort Flags;

                [FieldOffset(4)]
                public XINPUT_GAMEPAD Gamepad;

                [FieldOffset(16)]
                public XINPUT_VIBRATION Vibration;
            }

            // ex Battery.State
            [StructLayout(LayoutKind.Explicit)]
            public struct XINPUT_BATTERY_INFORMATION
            {
                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(0)]
                public byte BatteryType;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(1)]
                public byte BatteryLevel;
            }

            // ex Keystroke
            [StructLayout(LayoutKind.Explicit)]
            public struct XINPUT_KEYSTROKE
            {
                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(0)]
                public ushort VirtualKey;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(2)]
                public char Unicode;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(4)]
                public ushort Flags;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(5)]
                public byte UserIndex;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(6)]
                public byte HidCode;
            } // struct Keystroke

        } // class Native

        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        } // class Point

        public class PointF
        {
            public float X { get; set; }
            public float Y { get; set; }
        } // class Point
    } // class X
}
