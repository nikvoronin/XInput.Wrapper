// XInput.Wrapper by Nikolai Voronin
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

        #region // Polling Loop /////////////////////////////////////////////////////////////////////////////

        // TODO should recognize which controller is present
        public static void StartUpdate(Gamepad slot0, Gamepad slot1 = null, Gamepad slot2 = null, Gamepad slot3 = null)
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
            updateThread = new Thread(() => UpdateLoop(updateSlots, cts.Token));            
            updateThread.Start();
        }

        public static void StopUpdate()
        {
            cts?.Cancel();
        }

        static void UpdateLoop(List<Gamepad> updateSlots, CancellationToken token)
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
        /// Tests availability of the XInput_1.4 subsystem. 
        /// Should not call often! This one not cached.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                bool xinput_ready = false;
                try
                {
                    Gamepad.StatePacket state = new Gamepad.StatePacket();
                    Gamepad.Native.XInputGetState(0, ref state);
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

            public Battery.State batteryState;
            public Battery.State BatteryState { get { return batteryState; } }

            StatePacket state = new StatePacket();

            ButtonFlag lastButtonsState = 0;
            public ButtonFlag Buttons { get { return state.Current.Buttons; } }

            int packetNumber = -1;
            public int PacketNumber { get { return state.PacketNumber; } }
            
            internal Gamepad(int userIndex)
            {
                this.userIndex = userIndex;
            }

            public Battery.State UpdateBattery()
            {
                Native.XInputGetBatteryInformation(
                    userIndex,
                    (byte)Battery.At.Gamepad,
                    ref batteryState);

                return batteryState;
            }


            #region // Capabilities //////////////////////////////////////////////////////////////////////////////////

            public DeviceCapability Capability
            {
                get
                {
                    DeviceCapability caps = new DeviceCapability();
                    Native.XInputGetCapabilities(
                        userIndex,
                        0x00000001, // always GAMEPAD_FLAG,
                        ref caps);
                    return caps;
                }
            }

            public bool IsWireless
            {
                get { return Capability.Flags.HasFlag(CapabilityFlag.Wireless); }
            }

            public bool IsForceFeedback
            {
                get { return Capability.Flags.HasFlag(CapabilityFlag.ForceFeedback); }
            }

            #endregion


            #region // Buttons, sticks, thumbs, etc //////////////////////////////////////////////////////////////////


            public bool Up_down { get { return state.Current.IsButtonDown(ButtonFlag.Up); }}
            public bool Up_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.Up); }}

            public bool Down_down { get { return state.Current.IsButtonDown(ButtonFlag.Down); }}
            public bool Down_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.Down); }}

            public bool Left_down { get { return state.Current.IsButtonDown(ButtonFlag.Left); }}
            public bool Left_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.Left); }}

            public bool Right_down { get { return state.Current.IsButtonDown(ButtonFlag.Right); }}
            public bool Right_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.Right); }}

            public bool A_down { get { return state.Current.IsButtonDown(ButtonFlag.A); }}
            public bool A_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.A); }}

            public bool B_down { get { return state.Current.IsButtonDown(ButtonFlag.B); }}
            public bool B_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.B); }}

            public bool X_down { get { return state.Current.IsButtonDown(ButtonFlag.X); }}
            public bool X_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.X); }}

            public bool Y_down { get { return state.Current.IsButtonDown(ButtonFlag.Y); }}
            public bool Y_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.Y); }}

            public bool Back_down { get { return state.Current.IsButtonDown(ButtonFlag.Back); }}
            public bool Back_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.Back); }}

            public bool Start_down { get { return state.Current.IsButtonDown(ButtonFlag.Start); }}
            public bool Start_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.Start); }}

            public bool LBumper_down { get { return state.Current.IsButtonDown(ButtonFlag.LBumper); }}
            public bool LBumper_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.LBumper); }}

            public bool RBumper_down { get { return state.Current.IsButtonDown(ButtonFlag.RBumper); }}
            public bool RBumper_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.RBumper); }}

            public bool LStick_down { get { return state.Current.IsButtonDown(ButtonFlag.LStick); }}
            public bool LStick_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.LStick); }}

            public bool RStick_down { get { return state.Current.IsButtonDown(ButtonFlag.RStick); }}
            public bool RStick_up { get { return state.Current.IsButtonUp(lastButtonsState, ButtonFlag.RStick); }}

            public int LTrigger { get { return state.Current.LeftTrigger; } }

            public int RTrigger { get { return state.Current.RightTrigger; } }

            public float LTrigger_N
            {
                get { return LTrigger > LTrigger_Threshold ? LTrigger / 255f : 0f; }
            }

            public float RTrigger_N
            {
                get { return RTrigger > RTrigger_Threshold ? RTrigger / 255f : 0f; }
            }

            private bool IsDeadZone(Point pt, float ringDeadZone)
            {
                return ringDeadZone > Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
            }

            public PointF LStick_N
            {
                get {
                    bool isdz = IsDeadZone(LStick, LStick_DeadZone);
                    PointF p = new PointF()
                        {
                            X = isdz ? 0f : state.Current.LStickX / 32768f,
                            Y = isdz ? 0f : state.Current.LStickY / 32768f
                        };
                    return p;
                }
            }

            public Point LStick
            {
                get {
                    Point p = new Point()
                        {
                            X = state.Current.LStickX,
                            Y = state.Current.LStickY
                        };
                    return p;
                }
            }

            public PointF RStick_N
            {
                get {
                    bool isdz = IsDeadZone(RStick, RStick_DeadZone);
                    PointF p = new PointF()
                    {
                        X = isdz ? 0f : state.Current.RStickX / 32768f,
                        Y = isdz ? 0f : state.Current.RStickY / 32768f
                    };
                    return p;
                }
            }

            public Point RStick
            {
                get {
                    Point p = new Point()
                        {
                            X = state.Current.RStickX,
                            Y = state.Current.RStickY
                        };
                    return p;
                }
            }

            #endregion

            bool isConnected;
            public bool IsConnected { get { return isConnected; }}

            public bool Enable { set { Native.XInputEnable(value); } }

            /// <summary>
            /// Update gamepad data
            /// </summary>
            /// <returns>TRUE - if state changed (button pressed, gamepad dis|connected, etc)</returns>
            public bool Update()
            {
                bool isChanged = false;

                lastButtonsState = state.Current.Buttons;
                packetNumber = state.PacketNumber;
                int result =
                    Native.XInputGetState(userIndex, ref state);

                if (isConnected != (result == 0))
                {
                    isChanged = true;
                    isConnected = (result == 0);
                    if(ConnectionChanged != null)
                        OnConnectionChanged();
                }

                // TODO do not update often. Should update only for wireless. ?shall I add an update interval
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
                    StopVibrate();
                }
                else
                {
                    if (ffbL_IsActive && (now >= ffbL_StopTime))
                        StopVibrateLLow();
                    if (ffbR_IsActive && (now >= ffbR_StopTime))
                        StopVibrateRHi();
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

            VibrationSpeed vibra = new VibrationSpeed();

            bool ffbL_IsActive = false;
            bool ffbR_IsActive = false;
            DateTime ffbL_StopTime;
            DateTime ffbR_StopTime;

            /// <summary>
            /// ForceFeedback vibration. The left motor is the low-frequency rumble motor. The right motor is the high-frequency rumble motor. The two motors are not the same, and they create different vibration effects.
            /// </summary>
            /// <param name="leftLoPower">0 .. 1.0 (0-100%); -1 = ignore</param>
            /// <param name="rightHiPower">0 .. 1.0 (0-100%); -1 = ignore</param>
            /// <param name="durationLeft">in milliseconds; 0 = stop; -1 = ignore</param>
            /// <param name="durationRight">in milliseconds; 0 = stop; -1 = ignore</param>
            public void Vibrate(
                float leftLoPower   = -1f,
                float rightHiPower  = -1f,
                int   durationLeft  = -1,
                int   durationRight = -1  )
            {
                if (leftLoPower >= 0)
                    vibra.LLowSpeed = PowerToSpeed(leftLoPower);

                if (durationLeft > 0)
                    ffbL_StopTime = StopTimeFromNow(durationLeft);

                if (rightHiPower >= 0)
                    vibra.RHiSpeed = PowerToSpeed(rightHiPower);

                if (durationRight > 0)
                    ffbR_StopTime = StopTimeFromNow(durationRight);

                if (isConnected)
                    Native.XInputSetState(userIndex, ref vibra);

                if (durationLeft > -1)
                    ffbL_IsActive = durationLeft > 0;

                if (durationRight > -1)
                    ffbR_IsActive = durationRight > 0;
            } // Vibrate

            public ushort PowerToSpeed(float power)
            {
                return (ushort)(65535d * ((float)Math.Max(0d, Math.Min(1d, power))));
            }

            protected DateTime StopTimeFromNow(int duration)
            {
                return
                    DateTime.UtcNow.Add(
                        TimeSpan.FromTicks(duration * TimeSpan.TicksPerMillisecond));
            }

            public void Vibrate(float leftLoPower, float rightHiPower, int duration)
            {
                Vibrate(leftLoPower, rightHiPower, duration, duration);
            }

            public void VibrateLLow(float power, int duration)
            {
                Vibrate(power, 0, duration, 0);
            }

            public void VibrateRHi(float power, int duration)
            {
                Vibrate(0, power, 0, duration);
            }

            public void StopVibrate()
            {
                Vibrate(0, 0, 0, 0);
            }

            public void StopVibrateRHi()
            {
                Vibrate(-1, 0, -1, 0);
            }

            public void StopVibrateLLow()
            {
                Vibrate(0, -1, 0, -1);
            }
            #endregion


            internal class Native
            {
                [DllImport("xinput1_4.dll")]
                public static extern int XInputGetState
                (
                    int dwUserIndex,
                    ref XINPUT_STATE pState
                );

                [DllImport("xinput1_4.dll")]
                public static extern int XInputSetState
                (
                    int dwUserIndex,
                    ref XINPUT_VIBRATION pVibration
                );

                [DllImport("xinput1_4.dll")]
                public static extern int XInputGetCapabilities
                (
                    int dwUserIndex,
                    int dwFlags,
                    ref DeviceCapability pCapabilities
                );


                [DllImport("xinput1_4.dll")]
                public static extern int XInputGetBatteryInformation
                (
                    int dwUserIndex,
                    byte devType,
                    ref Battery.State pBatteryInformation
                );

                [DllImport("xinput1_4.dll")]
                public static extern int XInputGetKeystroke
                (
                    int dwUserIndex,
                    int dwReserved,
                    ref Keystroke pKeystroke
                );

                [DllImport("xinput1_4.dll")]
                public static extern void XInputEnable
                (
                    bool enable
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
            } // class Native


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
            public struct DeviceCapability
            {
                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(0)]
                public DeviceType IsGamepad;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(1)]
                public ControllerType Type;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(2)]
                public CapabilityFlag Flags;

                [FieldOffset(4)]
                public PadState Gamepad;

                [FieldOffset(16)]
                public VibrationSpeed ForceFeedback;
            } // struct Capability

            public enum DeviceType : byte
            {
                Gamepad = 0x01  // always gamepad
            }

            [Flags]
            public enum CapabilityFlag : short
            {
                VoiceSupport    = 0x0004,
                //Windows 8 and higher only
                ForceFeedback   = 0x0001,   // Device supports force feedback functionality.
                Wireless        = 0x0002,
                PMD_Supported   = 0x0008,   // Device supports plug-in modules.
                NoNavigation    = 0x0010,   // Device lacks menu navigation buttons (START, BACK, DPAD).
            };

            [StructLayout(LayoutKind.Explicit)]
            public struct PadState
            {
                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(0)]
                public ButtonFlag Buttons;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(2)]
                public byte LeftTrigger;

                [MarshalAs(UnmanagedType.I1)]
                [FieldOffset(3)]
                public byte RightTrigger;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(4)]
                public short LStickX;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(6)]
                public short LStickY;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(8)]
                public short RStickX;

                [MarshalAs(UnmanagedType.I2)]
                [FieldOffset(10)]
                public short RStickY;

                public bool IsButtonDown(ButtonFlag buttonMask)
                {
                    return (Buttons & buttonMask) == buttonMask;
                }

                public bool IsButtonUp(ButtonFlag lastButtonsState, ButtonFlag buttonMask)
                {
                    return
                        ((lastButtonsState & buttonMask) == buttonMask) &&
                        ((Buttons & buttonMask) != buttonMask);
                }

                public bool IsButtonPresent(ButtonFlag buttonMask)
                {
                    return (Buttons & buttonMask) == buttonMask;
                }

                public void Copy(PadState source)
                {
                    LStickX = source.LStickX;
                    LStickY = source.LStickY;
                    RStickX = source.RStickX;
                    RStickY = source.RStickY;
                    LeftTrigger = source.LeftTrigger;
                    RightTrigger = source.RightTrigger;
                    Buttons = source.Buttons;
                }

                public override int GetHashCode()
                {
                    return LStickX ^ LStickY ^ RStickX ^ RStickY ^ LeftTrigger ^ RightTrigger ^ (ushort)Buttons;
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is PadState))
                        return false;

                    PadState source = (PadState)obj;
                    return
                        ((LStickX == source.LStickX)
                            && (LStickY == source.LStickY)
                            && (RStickX == source.RStickX)
                            && (RStickY == source.RStickY)
                            && (LeftTrigger == source.LeftTrigger)
                            && (RightTrigger == source.RightTrigger)
                            && (Buttons == source.Buttons));
                }
            } // struct Pad

            [StructLayout(LayoutKind.Explicit)]
            public struct StatePacket
            {
                [FieldOffset(0)]
                public int PacketNumber;

                [FieldOffset(4)]
                public PadState Current;

                public void Copy(StatePacket source)
                {
                    PacketNumber = source.PacketNumber;
                    Current.Copy(source.Current);
                }

                public override bool Equals(object obj)
                {
                    if ((obj == null) || (!(obj is StatePacket)))
                        return false;
                    StatePacket source = (StatePacket)obj;

                    return ((PacketNumber == source.PacketNumber)
                        && (Current.Equals(source.Current)));
                }

                public override int GetHashCode()
                {
                    return PacketNumber;
                }
            } // struct PacketState

            [StructLayout(LayoutKind.Sequential)]
            public struct VibrationSpeed
            {
                [MarshalAs(UnmanagedType.I2)]
                public ushort LLowSpeed;

                [MarshalAs(UnmanagedType.I2)]
                public ushort RHiSpeed;

                public VibrationSpeed(ushort llowSpeed = 0 , ushort rhiSpeed = 0)
                {
                    LLowSpeed = llowSpeed;
                    RHiSpeed = rhiSpeed;
                }
            } // struct VibrationSpeed

            public class Battery
            {
                [StructLayout(LayoutKind.Explicit)]
                public struct State
                {
                    [MarshalAs(UnmanagedType.I1)]
                    [FieldOffset(0)]
                    public Type BatteryType;

                    [MarshalAs(UnmanagedType.I1)]
                    [FieldOffset(1)]
                    public Charge ChargeLevel;
                } // struct Information

                // Flags for battery status level
                public enum Type : byte
                {
                    Disconnected = 0x00,    // This device is not connected
                    Wired        = 0x01,    // Wired device, no battery
                    Alkaline     = 0x02,    // Alkaline battery source
                    NiMh         = 0x03,    // Nickel Metal Hydride battery source
                    Unknown      = 0xFF,    // Cannot determine the battery type
                };

                // These are only valid for wireless, connected devices, with known battery types
                // The amount of use time remaining depends on the type of device.
                public enum Charge : byte
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

            public enum ControllerType : byte
            {
                Unknown         = 0x00,
                Gamepad         = 0x01,
                Wheel           = 0x02,
                ArcadeStick     = 0x03,
                FlightStick     = 0x04,
                DancePad        = 0x05,
                Guitar          = 0x06,
                GuitarAlternate = 0x07,
                DrumKit         = 0x08,
                GuitarBass      = 0x0B,
                ArcadePad       = 0x13
            };

            [Flags]
            public enum ButtonFlag : ushort
            {
                Up      = 0x0001,
                Down    = 0x0002,
                Left    = 0x0004,
                Right   = 0x0008,
                Start   = 0x0010,
                Back    = 0x0020,
                LStick  = 0x0040,
                RStick  = 0x0080,
                LBumper = 0x0100,
                RBumper = 0x0200,
                A       = 0x1000,
                B       = 0x2000,
                X       = 0x4000,
                Y       = 0x8000,
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
    } // class X
}
