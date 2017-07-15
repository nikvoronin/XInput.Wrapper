using System;
using System.Collections.Generic;

namespace XInput.Wrapper
{
    public static partial class X
    {
        public sealed partial class Gamepad
        {
            public readonly uint Index;

            public readonly Battery GamepadBattery;
            public readonly Battery HeadsetBattery;

            Native.XINPUT_STATE state = new Native.XINPUT_STATE();

            uint packetNumber = 0;
            public uint PacketNumber { get { return state.dwPacketNumber; } }
            public bool SendKeyDownEveryTick { get; set; }

            public Button A;
            public ButtonFlags ButtonsState = ButtonFlags.None;
            internal readonly List<Button> Buttons = new List<Button>();  // TODO check access to the list if it became public
            // TODO Buttons by number
            //public readonly Dictionary<ushort, Button> ButtonsByNumber
            //public Button GetButton(int buttonNo)
            //{
            //    Button b;
            //    return b;
            //}

            internal Gamepad(uint index)
            {
                Index = index;
                GamepadBattery = new Battery(index, Battery.At.Gamepad);
                HeadsetBattery = new Battery(index, Battery.At.Headset);

                // UNDONE other buttons
                A = new Button(ButtonFlags.A);
                // TODO >README Add supported buttons only. should based on capabilities
                Buttons.Add(A);
            }

            bool isConnected;
            public bool IsConnected { get { return isConnected; } }
            public static bool Enable { set { Native.XInputEnable(value); } }

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
                    if (ConnectionChanged != null)
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
                if (isConnected)
                {
                    ButtonsState = (ButtonFlags)state.Gamepad.wButtons;

                    ButtonFlags downButtons = ButtonFlags.None;
                    ButtonFlags upButtons = ButtonFlags.None;

                    foreach (Button b in Buttons)
                    {
                        Button.Went wh = b.Update(ref state);

                        if (wh == Button.Went.Down ||
                            (SendKeyDownEveryTick && b.Pressed))
                        {
                            downButtons |= b.Mask;
                        }
                        else
                        {
                            if (wh == Button.Went.Up)
                                upButtons |= b.Mask;
                        } // else
                    } // foreach

                    if (downButtons != ButtonFlags.None)
                        OnKeyDown(downButtons);

                    if (upButtons != ButtonFlags.None)
                        OnKeyUp(upButtons);
                } // if isConnected

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
            public event EventHandler<KeyEventArgs> KeyDown;
            public event EventHandler<KeyEventArgs> KeyUp;

            public void OnStateChanged()
            {
                OnEvent(this, StateChanged);
            }

            public void OnConnectionChanged()
            {
                OnEvent(this, ConnectionChanged);
            }

            public void OnKeyDown(ButtonFlags buttons)
            {
                OnKeyEvent(this, KeyDown, buttons);
            }

            public void OnKeyUp(ButtonFlags buttons)
            {
                OnKeyEvent(this, KeyUp, buttons);
            }

            #endregion


            // TODO For binary state controls, such as digital buttons, the corresponding bit reflects whether or not the control is supported by the device. For proportional controls, such as thumbsticks, the value indicates the resolution for that control. Some number of the least significant bits may not be set, indicating that the control does not provide resolution to that level.
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
                    VoiceSupport = 0x0004,

                    //Windows 8 or higher only
                    ForceFeedback = 0x0001,   // Device supports force feedback functionality.
                    Wireless = 0x0002,
                    PMD_Supported = 0x0008,   // Device supports plug-in modules.
                    NoNavigation = 0x0010,   // Device lacks menu navigation buttons (START, BACK, DPAD).
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
        } // class Gamepad
    } // class X
}
