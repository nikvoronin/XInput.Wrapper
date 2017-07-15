namespace XInput.Wrapper
{
    public static partial class X
    {
        public sealed partial class Gamepad
        {
            public class Battery
            {
                readonly uint uindex;
                Native.XINPUT_BATTERY_INFORMATION state;

                public readonly At Location;
                public SourceType Type { get { return (SourceType)state.BatteryType; } }
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
                public bool Update()
                {
                    return Native.XInputGetBatteryInformation(uindex, (byte)Location, ref state) == 0;
                }

                public enum SourceType : byte
                {
                    Disconnected = 0x00,
                    WiredNoBattery = 0x01,
                    Alkaline = 0x02,
                    NiMh = 0x03,
                    Unknown = 0xFF,
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

        } // class Gamepad
    } // class X
}
