using System;
using System.Text;
using System.Runtime.InteropServices;

namespace FatRead.Tests
{
    /// <summary>
    /// Test data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TestDataStruct
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]

        public byte[] Name;
        [FieldOffset(11)]
        public bool ByteVal;

        [FieldOffset(12)]
        public int IntValue;

        [FieldOffset(16)]
        public long BigIntValue;

        [FieldOffset(24)]
        public double FloatValue;
    }
}