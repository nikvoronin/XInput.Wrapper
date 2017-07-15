using System;
using System.Collections.Generic;

namespace XInput.Wrapper
{
    public static partial class X
    {
        public sealed partial class Gamepad
        {
            public class Button
            {
                public readonly ButtonFlags Mask;
                public readonly string Name = string.Empty;
                public bool Supported { get; internal set; } // TODO fill in the flag based on the capabilities
                public bool SendKeyDownEveryTick { get; set; }

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

                    if (Pressed &&
                        SendKeyDownEveryTick &&
                        lastAct != Went.Down &&
                        KeyDown != null)
                    {
                        OnKeyDown();
                    }

                    // TODO >README first occur many Button.KeyDown|Up events and only after that occur one global event KeyDown|Up with buttons mask

                    return lastAct;
                } // Update

                public event EventHandler<KeyEventArgs> KeyDown;
                public event EventHandler<KeyEventArgs> KeyUp;

                public void OnKeyDown()
                {
                    OnKeyEvent(this, KeyDown, Mask);
                }

                public void OnKeyUp()
                {
                    OnKeyEvent(this, KeyUp, Mask);
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
                    { ButtonFlags.LThumb, "Left_Thumb" },
                    { ButtonFlags.RThumb, "Right_Thumb" },
                    { ButtonFlags.LBumper, "Left_Bumper" },
                    { ButtonFlags.RBumper, "Right_Bumper" },
                    { ButtonFlags.LTopShoulder, "LeftTop_Shoulder" },
                    { ButtonFlags.RTopShoulder, "RightTop_Shoulder" },
                    { ButtonFlags.A, "Button_A" },
                    { ButtonFlags.B, "Button_B" },
                    { ButtonFlags.X, "Button_X" },
                    { ButtonFlags.Y, "Button_Y" }
                };

                public readonly Dictionary<ButtonFlags, ushort> Numbers = new Dictionary<ButtonFlags, ushort>() {
                    { ButtonFlags.Up, 12 },
                    { ButtonFlags.Down, 13 },
                    { ButtonFlags.Left, 14 },
                    { ButtonFlags.Right, 15 },
                    { ButtonFlags.Start, 9 },
                    { ButtonFlags.Back, 10 },
                    { ButtonFlags.LStick, 7 },
                    { ButtonFlags.RStick, 8 },
                    { ButtonFlags.LThumb, 7 },
                    { ButtonFlags.RThumb, 8 },
                    { ButtonFlags.LBumper, 5 },
                    { ButtonFlags.RBumper, 6 },
                    { ButtonFlags.LTopShoulder, 5 },
                    { ButtonFlags.RTopShoulder, 6 },
                    { ButtonFlags.A, 1 },
                    { ButtonFlags.B, 2 },
                    { ButtonFlags.X, 3 },
                    { ButtonFlags.Y, 4 }
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
                LThumb = 0x0040,
                RThumb = 0x0080,
                LBumper = 0x0100,
                RBumper = 0x0200,
                LTopShoulder = 0x0100,
                RTopShoulder = 0x0200,
                A = 0x1000,
                B = 0x2000,
                X = 0x4000,
                Y = 0x8000,
            };

        } // class Gamepad
    } // class X
}
