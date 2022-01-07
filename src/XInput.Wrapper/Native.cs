using System.Runtime.InteropServices;

namespace XInput.Wrapper
{
    public static partial class X
    {
        private const string XINPUT1_4_DLL = "xinput1_4.dll";

        public static class Native
        {
            [DllImport(XINPUT1_4_DLL)]
            public static extern uint XInputGetState (
                uint dwUserIndex,
                ref XINPUT_STATE pState 
                );

            [DllImport(XINPUT1_4_DLL)]
            public static extern uint XInputSetState (
                uint dwUserIndex,
                ref XINPUT_VIBRATION pVibration
                );

            [DllImport(XINPUT1_4_DLL)]
            public static extern uint XInputGetCapabilities (
                uint dwUserIndex,
                uint dwFlags,
                ref XINPUT_CAPABILITIES pCapabilities
                );

            [DllImport(XINPUT1_4_DLL)]
            public static extern uint XInputGetBatteryInformation (
                uint dwUserIndex,
                byte devType,
                ref XINPUT_BATTERY_INFORMATION pBatteryInformation
                );

            [DllImport(XINPUT1_4_DLL)]
            public static extern uint XInputGetKeystroke (
                uint dwUserIndex,
                uint dwReserved,
                ref XINPUT_KEYSTROKE pKeystroke
                );

            [DllImport(XINPUT1_4_DLL)]
            public static extern void XInputEnable (
                bool enable
                );

            [DllImport( XINPUT1_4_DLL )]
            public static extern uint XInputGetAudioDeviceIds (
                uint dwUserIndex,
                [MarshalAs(UnmanagedType.LPWStr)]out string pRenderDeviceId,
                ref uint pRenderCount,
                [MarshalAs(UnmanagedType.LPWStr)]out string pCaptureDeviceId,
                ref uint pCaptureCount
                );

            [StructLayout(LayoutKind.Explicit)]
            public struct XINPUT_STATE
            {
                [FieldOffset(0)]
                public uint dwPacketNumber;

                [FieldOffset(4)]
                public XINPUT_GAMEPAD Gamepad;
            }

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

            [StructLayout(LayoutKind.Sequential)]
            public struct XINPUT_VIBRATION
            {
                [MarshalAs(UnmanagedType.I2)]
                public ushort wLeftMotorSpeed;

                [MarshalAs(UnmanagedType.I2)]
                public ushort wRightMotorSpeed;
            }

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
            }

            public const uint ERROR_DEVICE_NOT_CONNECTED = 0x048F; // Winerror.h
        }
    }
}
