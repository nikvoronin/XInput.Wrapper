using System;
using System.Collections.Generic;

namespace XInput.Wrapper
{
    public static partial class X
    {
        public sealed partial class Gamepad
        {
            public readonly uint Index; // User index or controller index, zero based in a range of [0..3]
            public uint UserId => Index;
            /// <summary>
            /// Each controller displays which ID it is using by lighting up a quadrant on the "ring of light" in the center of the controller. A dwUserIndex value of 0 corresponds to the top-left quadrant; the numbering proceeds around the ring in clockwise order.
            /// </summary>
            public Quadrant UserQuadrant => (Quadrant)Index;

            public readonly Battery GamepadBattery;
            public readonly Battery HeadsetBattery;

            //uint packetNumber = 0;
            public uint PacketNumber { get { return _internalState.dwPacketNumber; } }
            public bool SendKeyDownEveryTick { get; set; }

            public Button A;
            public ButtonFlags ButtonsState = ButtonFlags.None;
            internal readonly List<Button> Buttons = new List<Button>();  // TODO yield Buttons.List
            // TODO Buttons by number
            //public readonly Dictionary<ushort, Button> ButtonsByNumber
            //public Button GetButton(int buttonNo)
            //{
            //    Button b;
            //    return b;
            //}

            private Native.XINPUT_STATE _internalState = new Native.XINPUT_STATE();

            internal Gamepad(uint index)
            {
                if (index < 0 || index > 3)
                    throw new ArgumentOutOfRangeException("index", index, "The XInput API supports up to four controllers. The index must be in range of [0..3]");

                Index = index;
                GamepadBattery = new Battery(Index, Battery.At.Gamepad);
                HeadsetBattery = new Battery(Index, Battery.At.Headset);

                // UNDONE other buttons
                A = new Button(ButtonFlags.A);
                // TODO >README Add supported buttons only. should based on capabilities
                Buttons.Add(A);
            }

            public bool Available => Connected;
            public bool Connected { get; internal set; }
            public static bool Enable { set { Native.XInputEnable(value); } }

            public bool UpdateConnectionState()
            {
                bool isChanged = false;
                uint result = Native.XInputGetState(Index, ref _internalState);

                if (Connected != (result == 0)) {
                    isChanged = true;

                    Connected = (result == 0);
                    OnConnectionStateChanged();
                }

                return isChanged;
            }

            /// <summary>
            /// Update gamepad data
            /// </summary>
            /// <returns>TRUE - if state has been changed (button pressed, gamepad dis|connected, etc)</returns>
            public bool Update()
            {
                //lastButtonsState = state.Current.Buttons;
                uint prevPacketNumber = _internalState.dwPacketNumber;
                bool isChanged = UpdateConnectionState();

                // UNDONE do not update often. Should update only for wireless. ?shall I add an update interval
                //if (isConnected)
                //    UpdateBattery();

                if (prevPacketNumber != _internalState.dwPacketNumber)
                {
                    isChanged = true;
                    if (StateChanged != null)
                        OnStateChanged();
                }

                // UNDONE  make list of axises  and update it
                if (Connected)
                {
                    ButtonsState = (ButtonFlags)_internalState.Gamepad.wButtons;

                    ButtonFlags downButtons = ButtonFlags.None;
                    ButtonFlags upButtons = ButtonFlags.None;

                    foreach (Button b in Buttons)
                    {
                        Button.Action act = b.Update(ref _internalState);

                        if (act == Button.Action.Down ||
                            (SendKeyDownEveryTick && b.Pressed))
                        {
                            downButtons |= b.Mask;
                        }
                        else
                        {
                            if (act == Button.Action.Up)
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

            public event EventHandler ConnectionStateChanged;
            public event EventHandler StateChanged;
            public event EventHandler<KeyEventArgs> KeyDown;
            public event EventHandler<KeyEventArgs> KeyUp;

            public void OnStateChanged()
            {
                OnEvent(this, StateChanged);
            }

            public void OnConnectionStateChanged()
            {
                OnEvent(this, ConnectionStateChanged);
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
            #region // Capability ////////////////////////////////////////////////////////////////////
            Native.XINPUT_CAPABILITIES caps;

            /// <summary>
            /// Update capabilities. Done automatically when controller is connected.
            /// </summary>
            /// <returns>TRUE - if updated successfully</returns>
            private bool UpdateCapabilities()
            {
                return Native.XInputGetCapabilities( Index,
                    0x00000001, // GAMEPAD_FLAG always
                    ref caps) == 0;
            }

            // TODO remove Is_ prefix
            public SubType PadType { get { return (SubType)caps.SubType; } }
            public bool IsWireless { get { return ((Capabilities)caps.Flags).HasFlag(Capabilities.Wireless); } }
            public bool IsForceFeedback { get { return ((Capabilities)caps.Flags).HasFlag(Capabilities.ForceFeedback); } }
            public bool IsVoiceSupport { get { return ((Capabilities)caps.Flags).HasFlag(Capabilities.VoiceSupport); } }
            public bool IsNoNavigation { get { return ((Capabilities)caps.Flags).HasFlag(Capabilities.NoNavigation); } }
            public bool IsPluginModules { get { return ((Capabilities)caps.Flags).HasFlag(Capabilities.PMD_Supported); } }

            [Flags]
            public enum Capabilities : ushort
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
            #endregion

            public enum Quadrant { TopLeft = 0, TopRight = 1, BottomRight = 2, BottomLeft = 3 }
        } // class Gamepad
    } // class X
}
