// XInput.Wrapper by Nikolai Voronin
// http://github.com/nikvoronin/xinput.wrapper
// Version 0.3.1 (June 16, 2016)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace XInput.Wrapper
{
    public static class X
    {
        static Thread pollingThread;
        static SynchronizationContext uiContext;
        static CancellationTokenSource cts;

        public static int UpdatesPerSecond = 30;

        public static readonly Gamepad Gamepad_1 = new Gamepad(0);
        public static readonly Gamepad Gamepad_2 = new Gamepad(1);
        public static readonly Gamepad Gamepad_3 = new Gamepad(2);
        public static readonly Gamepad Gamepad_4 = new Gamepad(3);

        static X() { }

        #region // Polling Loop /////////////////////////////////////////////////////////////////////////////

        public static void StartPolling(Gamepad slot0, Gamepad slot1 = null, Gamepad slot2 = null, Gamepad slot3 = null)
        {
            List<Gamepad> updateSlots = new List<Gamepad>();
            updateSlots.Add(slot0);
            if (slot1 != null)
                updateSlots.Add(slot1);
            if (slot2 != null)
                updateSlots.Add(slot2);
            if (slot3 != null)
                updateSlots.Add(slot3);

            cts = new CancellationTokenSource();
            uiContext = SynchronizationContext.Current;
            pollingThread = new Thread(() => PollingLoop(updateSlots, cts.Token));            
            pollingThread.Start();
        }

        public static void StopPolling()
        {
            cts?.Cancel();
        }

        static void PollingLoop(List<Gamepad> updateSlots, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (Gamepad pad in updateSlots)
                {
                    if (token.IsCancellationRequested)
                        break;

                    pad.Update();
                }

                if (token.IsCancellationRequested)
                    break;
                else
                    Thread.Sleep(1000 / UpdatesPerSecond);
            }
        }
        #endregion


        /// <summary>
        /// Should not call often! This one not cached.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                bool xinput_ready = false;
                try
                {
                    Gamepad.PacketState state = new Gamepad.PacketState();
                    Gamepad.InputWrapper.XInputGetState(0, ref state);
                    xinput_ready = true;
                }
                catch
                {
                    xinput_ready = false;
                }

                return xinput_ready;
            }
        }

        public class Gamepad
        {
            public int LStick_DeadZone = 7849;
            public int RStick_DeadZone = 8689;
            public int LTrigger_Threshold = 30;
            public int RTrigger_Threshold = 30;

            private readonly int userIndex;

            Battery.Information batteryInfo;
            public Battery.Information BatteryInfo { get { return batteryInfo; } }

            short buttons = 0;
            public short Buttons { get { return state.Gamepad.wButtons; } }
            int packetNumber = -1;
            public int PacketNumber { get { return state.PacketNumber; } }
            
            PacketState state = new PacketState();

            internal Gamepad(int userIndex)
            {
                this.userIndex = userIndex;
            }

            public void UpdateBattery()
            {
                Battery.Information gamepad = new Battery.Information();

                InputWrapper.XInputGetBatteryInformation(
                    userIndex,
                    (byte)Battery.At.Gamepad,
                    ref gamepad);

                batteryInfo = gamepad;
            }

            public Capability Capabilities
            {
                get
                {
                    Capability capabilities = new Capability();
                    InputWrapper.XInputGetCapabilities(
                        userIndex,
                        GAMEPAD_FLAG,
                        ref capabilities);
                    return capabilities;
                }
            }


            #region // Buttons, sticks, thumbs, etc //////////////////////////////////////////////////////////////////


            public bool Dpad_Up_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.Dpad_Up); }}
            public bool Dpad_Up_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.Dpad_Up); }}

            public bool Dpad_Down_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.Dpad_Down); }}
            public bool Dpad_Down_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.Dpad_Down); }}

            public bool Dpad_Left_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.Dpad_Left); }}
            public bool Dpad_Left_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.Dpad_Left); }}

            public bool Dpad_Right_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.Dpad_Right); }}
            public bool Dpad_Right_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.Dpad_Right); }}

            public bool A_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.A); }}
            public bool A_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.A); }}

            public bool B_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.B); }}
            public bool B_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.B); }}

            public bool X_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.X); }}
            public bool X_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.X); }}

            public bool Y_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.Y); }}
            public bool Y_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.Y); }}

            public bool Back_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.Back); }}
            public bool Back_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.Back); }}

            public bool Start_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.Start); }}
            public bool Start_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.Start); }}

            public bool LBumper_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.LBumper); }}
            public bool LBumper_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.LBumper); }}

            public bool RBumper_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.RBumper); }}
            public bool RBumper_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.RBumper); }}

            public bool LStick_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.LeftStick); }}
            public bool LStick_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.LeftStick); }}

            public bool RStick_down { get { return state.Gamepad.IsButtonDown(GamepadButtons.RightStick); }}
            public bool RStick_up { get { return state.Gamepad.IsButtonUp(buttons, GamepadButtons.RightStick); }}

            public int LTrigger { get { return state.Gamepad.bLeftTrigger; } }

            public int RTrigger { get { return state.Gamepad.bRightTrigger; } }

            public float LTrigger_N
            {
                get
                {
                    return
                        LTrigger > LTrigger_Threshold ?
                            LTrigger / 255f :
                            0;
                }
            }

            public float RTrigger_N
            {
                get
                {
                    return
                        RTrigger > RTrigger_Threshold ?
                            RTrigger / 255f :
                            0;
                }
            }

            private bool IsDeadZone(Point pt, float ringDeadZone)
            {
                return ringDeadZone > Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
            }

            public PointF LStick_N
            {
                get
                {
                    bool isdz = IsDeadZone(LStick, LStick_DeadZone);
                    PointF p = new PointF()
                        {
                            X = isdz ? 0f : state.Gamepad.sThumbLX / 32768f,
                            Y = isdz ? 0f : state.Gamepad.sThumbLY / 32768f
                    };
                    return p;
                }
            }

            public Point LStick
            {
                get
                {
                    Point p = new Point()
                        {
                            X = state.Gamepad.sThumbLX,
                            Y = state.Gamepad.sThumbLY
                        };
                    return p;
                }
            }

            public PointF RStick_N
            {
                get
                {
                    bool isdz = IsDeadZone(RStick, RStick_DeadZone);
                    PointF p = new PointF()
                    {
                        X = isdz ? 0f : state.Gamepad.sThumbRX / 32768f,
                        Y = isdz ? 0f : state.Gamepad.sThumbRY / 32768f
                    };
                    return p;
                }
            }

            public Point RStick
            {
                get
                {
                    Point p = new Point()
                        {
                            X = state.Gamepad.sThumbRX,
                            Y = state.Gamepad.sThumbRY
                        };
                    return p;
                }
            }

            #endregion

            bool isConnected;
            public bool IsConnected { get { return isConnected; }}

            public bool Enable { set { InputWrapper.XInputEnable(value); } }

            /// <summary>
            /// Update gamepad data
            /// </summary>
            /// <returns>TRUE - if state changed (button pressed, gamepad dis|connected, etc)</returns>
            public bool Update()
            {
                bool isChanged = false;

                buttons = state.Gamepad.wButtons;
                packetNumber = state.PacketNumber;
                int result = InputWrapper.XInputGetState(userIndex, ref state);

                if (isConnected != (result == 0))
                {
                    isChanged = true;
                    isConnected = (result == 0);
                    if(ConnectionChanged != null)
                        OnConnectionChanged();
                }

                if (isConnected)
                    UpdateBattery();

                if (state.PacketNumber != packetNumber)
                {
                    isChanged = true;
                    if (StateChanged != null)
                        OnStateChanged();
                }

                // KeyDown is a long event
                if ((Buttons != 0) && (KeyDown != null))
                    OnKeyDown();

                // Force feedback
                DateTime now = DateTime.UtcNow;
                if ((ffbL_IsActive && (now >= ffbL_StopTime)) &&
                    (ffbR_IsActive && (now >= ffbR_StopTime)))
                {
                    FFB_Stop();
                }
                else
                {
                    if (ffbL_IsActive && (now >= ffbL_StopTime))
                        FFB_StopLeft();
                    if (ffbR_IsActive && (now >= ffbR_StopTime))
                        FFB_StopRight();
                }

                return isChanged;
            } // UpdateState()


            #region // Events ////////////////////////////////////////////////////////////////////


            public event EventHandler ConnectionChanged;
            protected virtual void __OnConnectionChanged(object o)
            {
                EventHandler pceh = ConnectionChanged;
                pceh?.Invoke(this, EventArgs.Empty);
            }

            public void OnConnectionChanged()
            {
                if (uiContext != null)
                    uiContext.Post(__OnConnectionChanged, null);
                else
                    __OnConnectionChanged(null);
            }

            public event EventHandler StateChanged;
            protected virtual void __OnStateChanged(object o)
            {
                EventHandler pceh = StateChanged;
                pceh?.Invoke(this, EventArgs.Empty);
            }

            public void OnStateChanged()
            {
                if (uiContext != null)
                    uiContext.Post(__OnStateChanged, null);
                else
                    __OnStateChanged(null);
            }

            public event EventHandler KeyDown;
            protected virtual void __OnKeyDown(object o)
            {
                EventHandler pceh = KeyDown;
                pceh?.Invoke(this, EventArgs.Empty);
            }

            public void OnKeyDown()
            {
                if (uiContext != null)
                    uiContext.Post(__OnKeyDown, null);
                else
                    __OnKeyDown(null);
            }
            #endregion

            #region // Force Feedback //////////////////////////////////////////////////////////////////////////

            Vibration vibration =
                new Vibration()
                {
                    LSpeed = 0,
                    RSpeed = 0
                };

            bool ffbL_IsActive = false;
            bool ffbR_IsActive = false;
            DateTime ffbL_StopTime;
            DateTime ffbR_StopTime;

            public void FFB_Vibrate(float leftLoMotor, float rightHiMotor, int duration)
            {
                FFB_Vibrate(leftLoMotor, rightHiMotor, duration, duration);
            }

            public void FFB_Vibrate(float leftLoMotor, float rightHiMotor, int durationLeft, int durationRight)
            {
                leftLoMotor = (float)Math.Max(0d, Math.Min(1d, leftLoMotor));
                rightHiMotor = (float)Math.Max(0d, Math.Min(1d, rightHiMotor));

                vibration.LSpeed = (ushort)(65535d * leftLoMotor);
                vibration.RSpeed = (ushort)(65535d * rightHiMotor);

                ffbL_StopTime = DateTime.UtcNow.Add(TimeSpan.FromTicks(durationLeft * TimeSpan.TicksPerMillisecond));
                ffbR_StopTime = DateTime.UtcNow.Add(TimeSpan.FromTicks(durationRight * TimeSpan.TicksPerMillisecond));

                if (isConnected)
                    InputWrapper.XInputSetState(userIndex, ref vibration);
                ffbL_IsActive = true;
                ffbR_IsActive = true;
            }

            public void FFB_LeftMotor(float leftLowFrequency, int duration)
            {
                leftLowFrequency = (float)Math.Max(0d, Math.Min(1d, leftLowFrequency));
                vibration.LSpeed = (ushort)(65535d * leftLowFrequency);
                ffbL_StopTime = DateTime.UtcNow.Add(TimeSpan.FromTicks(duration * TimeSpan.TicksPerMillisecond));

                if (isConnected)
                    InputWrapper.XInputSetState(userIndex, ref vibration);
                ffbL_IsActive = true;
            }

            public void FFB_RightMotor(float rightHiFrequency, int duration)
            {
                rightHiFrequency = (float)Math.Max(0d, Math.Min(1d, rightHiFrequency));
                vibration.RSpeed = (ushort)(65535d * rightHiFrequency);
                ffbR_StopTime = DateTime.UtcNow.Add(TimeSpan.FromTicks(duration * TimeSpan.TicksPerMillisecond));

                if (isConnected)
                    InputWrapper.XInputSetState(userIndex, ref vibration);
                ffbR_IsActive = true;
            }

            public void FFB_StopLeft()
            {
                vibration.LSpeed = 0;

                if(isConnected)
                    InputWrapper.XInputSetState(userIndex, ref vibration);
                ffbL_IsActive = false;
            }

            public void FFB_StopRight()
            {
                vibration.RSpeed = 0;

                if (isConnected)
                    InputWrapper.XInputSetState(userIndex, ref vibration);
                ffbR_IsActive = false;
            }

            public void FFB_Stop()
            {
                vibration.LSpeed = 0;
                vibration.RSpeed = 0;

                if (isConnected)
                    InputWrapper.XInputSetState(userIndex, ref vibration);
                ffbL_IsActive = false;
                ffbR_IsActive = false;
            }
            #endregion


            internal class InputWrapper
            {
                [DllImport("xinput1_4.dll")]
                public static extern int XInputGetState
                (
                    int dwUserIndex,  // [in] Index of the gamer associated with the device
                    ref PacketState pState        // [out] Receives the current state
                );

                [DllImport("xinput1_4.dll")]
                public static extern int XInputSetState
                (
                    int dwUserIndex,  // [in] Index of the gamer associated with the device
                    ref Vibration pVibration    // [in, out] The vibration information to send to the controller
                );

                [DllImport("xinput1_4.dll")]
                public static extern int XInputGetCapabilities
                (
                    int dwUserIndex,   // [in] Index of the gamer associated with the device
                    int dwFlags,       // [in] Input flags that identify the device type
                    ref Capability pCapabilities  // [out] Receives the capabilities
                );


                [DllImport("xinput1_4.dll")]
                public static extern int XInputGetBatteryInformation
                (
                    int dwUserIndex,        // Index of the gamer associated with the device
                    byte devType,            // Which device on this user index
                    ref Battery.Information pBatteryInformation // Contains the level and types of batteries
                );

                [DllImport("xinput1_4.dll")]
                public static extern int XInputGetKeystroke
                (
                    int dwUserIndex,              // Index of the gamer associated with the device
                    int dwReserved,               // Reserved for future use
                    ref Keystroke pKeystroke    // Pointer to an XINPUT_KEYSTROKE structure that receives an input event.
                );

                [DllImport("xinput1_4.dll")]
                public static extern void XInputEnable
                (
                    bool enable
                );
            } // class InputWrapper


            #region // Structures ///////////////////////////////////////////////////////////////


            [StructLayout(LayoutKind.Explicit)]
            public struct Keystroke
            {
                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(0)]
                public short VirtualKey;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(2)]
                public char Unicode;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(4)]
                public short Flags;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(5)]
                public byte UserIndex;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(6)]
                public byte HidCode;
            } // struct Keystroke

            [StructLayout(LayoutKind.Explicit)]
            public struct Capability
            {
                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(0)]
                public DeviceType Type;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(1)]
                public DeviceSubType SubType;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(2)]
                public CapabilityFlags Flags;

                [FieldOffset(4)]
                public Pad Gamepad;

                [FieldOffset(16)]
                public Vibration Vibration;
            } // struct Capability

            public enum DeviceType : byte
            {
                Gamepad = 0x01
            }

            [Flags]
            public enum CapabilityFlags : short
            {
                VoiceSupport = 0x0004,
                //Windows 8 only
                FFB_Supported = 0x0001, // Device supports force feedback functionality.
                Wireless = 0x0002,
                PMD_Supported = 0x0008, // Device supports plug-in modules.
                NoNavigation = 0x0010,  // Device lacks menu navigation buttons (START, BACK, DPAD).
            };

            public const int GAMEPAD_FLAG = 0x00000001;

            [StructLayout(LayoutKind.Explicit)]
            public struct Pad
            {
                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(0)]
                public short wButtons;

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

                public bool IsButtonDown(GamepadButtons buttonFlags)
                {
                    return (wButtons & (int)buttonFlags) == (int)buttonFlags;
                }

                public bool IsButtonUp(short prevButtons, GamepadButtons buttonFlags)
                {
                    return
                        ((prevButtons & (int)buttonFlags) == (int)buttonFlags) &&
                        ((wButtons & (int)buttonFlags) != (int)buttonFlags);
                }

                public bool IsButtonPresent(GamepadButtons buttonFlags)
                {
                    return (wButtons & (int)buttonFlags) == (int)buttonFlags;
                }

                public void Copy(Pad source)
                {
                    sThumbLX = source.sThumbLX;
                    sThumbLY = source.sThumbLY;
                    sThumbRX = source.sThumbRX;
                    sThumbRY = source.sThumbRY;
                    bLeftTrigger = source.bLeftTrigger;
                    bRightTrigger = source.bRightTrigger;
                    wButtons = source.wButtons;
                }

                public override int GetHashCode()
                {
                    return sThumbLX ^ sThumbLY ^ sThumbRX ^ sThumbRY ^ bLeftTrigger ^ bRightTrigger ^ wButtons;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is Pad))
                        return false;
                    Pad source = (Pad)obj;
                    return ((sThumbLX == source.sThumbLX)
                    && (sThumbLY == source.sThumbLY)
                    && (sThumbRX == source.sThumbRX)
                    && (sThumbRY == source.sThumbRY)
                    && (bLeftTrigger == source.bLeftTrigger)
                    && (bRightTrigger == source.bRightTrigger)
                    && (wButtons == source.wButtons));
                }
            } // struct Pad

            [StructLayout(LayoutKind.Explicit)]
            public struct PacketState
            {
                [FieldOffset(0)]
                public int PacketNumber;

                [FieldOffset(4)]
                public Pad Gamepad;

                public void Copy(PacketState source)
                {
                    PacketNumber = source.PacketNumber;
                    Gamepad.Copy(source.Gamepad);
                }

                public override bool Equals(object obj)
                {
                    if ((obj == null) || (!(obj is PacketState)))
                        return false;
                    PacketState source = (PacketState)obj;

                    return ((PacketNumber == source.PacketNumber)
                        && (Gamepad.Equals(source.Gamepad)));
                }

                public override int GetHashCode()
                {
                    return PacketNumber;
                }
            } // struct PacketState

            [StructLayout(LayoutKind.Sequential)]
            public struct Vibration
            {
                [MarshalAs(UnmanagedType.I2)]
                public ushort LSpeed;

                [MarshalAs(UnmanagedType.I2)]
                public ushort RSpeed;
            } // struct Vibration

            public class Battery
            {
                [StructLayout(LayoutKind.Explicit)]
                public struct Information
                {
                    [MarshalAs(UnmanagedType.I1)]
                    [FieldOffset(0)]
                    public Types BatteryType;

                    [MarshalAs(UnmanagedType.I1)]
                    [FieldOffset(1)]
                    public ChargeLevel ChargeLevel;

                    public override string ToString()
                    {
                        return string.Format("{0} {1}", BatteryType, ChargeLevel);
                    }
                } // struct Information

                // Flags for battery status level
                public enum Types : byte
                {
                    Disconnected = 0x00,    // This device is not connected
                    Wired = 0x01,    // Wired device, no battery
                    Alkaline = 0x02,    // Alkaline battery source
                    NiMh = 0x03,    // Nickel Metal Hydride battery source
                    Unknown = 0xFF,    // Cannot determine the battery type
                };

                // These are only valid for wireless, connected devices, with known battery types
                // The amount of use time remaining depends on the type of device.
                public enum ChargeLevel : byte
                {
                    Empty = 0x00,
                    Low = 0x01,
                    Medium = 0x02,
                    Full = 0x03
                };

                public enum At : byte
                {
                    Gamepad = 0x00,
                    Headset = 0x01,
                }
            } // class Battery

            [Flags]
            public enum DeviceSubType : byte
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

            [Flags]
            public enum GamepadButtons : int
            {
                Dpad_Up = 0x0001,
                Dpad_Down = 0x0002,
                Dpad_Left = 0x0004,
                Dpad_Right = 0x0008,
                Start = 0x0010,
                Back = 0x0020,
                LeftStick = 0x0040,
                RightStick = 0x0080,
                LBumper = 0x0100,
                RBumper = 0x0200,
                A = 0x1000,
                B = 0x2000,
                X = 0x4000,
                Y = 0x8000,
            };
            #endregion
        } // class Gamepad

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
    } // class XInput
}
