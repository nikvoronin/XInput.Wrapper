using System;
using System.Collections.Generic;

namespace XInput.Wrapper
{
    public static partial class X
    {
        public sealed partial class Gamepad
        {
            public class Axis
            {
                public readonly AxisFlags Mask;
                public readonly string Name = string.Empty;

                public short X { get; }
                public short Y { get; }

                public uint DeadZoneRadius { get; set; }

                public readonly short MinX = short.MinValue;
                public readonly short MaxX = short.MaxValue;
                public readonly short MinY = short.MinValue;
                public readonly short MaxY = short.MaxValue;

                // Normalized float X: 0.0f .. 1.0f, returns 0.0f when axis is in a dead zone.
                public float Xn { get; }
                // Normalized float Y: 0.0f .. 1.0f, returns 0.0f when axis is in a dead zone.
                public float Yn { get; }

                // Precise magnitude calculation
                public float Magnitude => (float)Math.Sqrt(Xn * Xn + Yn * Yn);

                // Fast and approximate: least square error w/ zero median  (Δmax=0.08158851)
                public float MagnitudeFast
                {
                    get
                    {
                        const float alpha = 0.948059f;
                        const float beta = 0.392699f;

                        float xa = Math.Abs(Xn);
                        float ya = Math.Abs(Yn);
                        if (xa > ya)
                            return alpha * xa + beta * ya;
                        else
                            return alpha * ya + beta * xa;
                    }
                }

                internal Axis(AxisFlags mask, short minX, short maxX, short minY, short maxY)
                {
                    Mask = mask;

                    if (Names.ContainsKey(mask))
                        Name = Names[mask];

                    MinX = minX;
                    MaxX = maxX;
                    MinY = minY;
                    MaxY = maxY;
                }

                internal Axis(AxisFlags mask)
                {
                    Mask = mask;

                    if (Names.ContainsKey(mask))
                        Name = Names[mask];
                }

                // UNDONE Update
                internal bool Update(Native.XINPUT_GAMEPAD gamepadState)
                {
                    // STUB Update.return
                    return false;
                }

                public readonly Dictionary<AxisFlags, string> Names = new Dictionary<AxisFlags, string>() {
                    { AxisFlags.None, "No_axis" },
                    { AxisFlags.LStick, "Left_ThumbStick" },
                    { AxisFlags.RStick, "Right_ThumbStick" },
                    { AxisFlags.LTrigger, "LeftBottom_ShoulderTrigger" },
                    { AxisFlags.RTrigger, "RightBottom_ShoulderTrigger" },
                };
            } // class Axis

            [Flags]
            public enum AxisFlags : uint
            {
                None = 0x0000,
                LStick = 0x0040,
                RStick = 0x0080,
                LThumb = 0x0040,
                RThumb = 0x0080,
                LTrigger = 0x0100,
                RTrigger = 0x0200,
                LBottomShoulder = 0x0100,
                RBottomShoulder = 0x0200,
            };

        } // class Gamepad
    } // class X
}
