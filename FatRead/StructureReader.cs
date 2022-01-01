using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FatRead
{
    /// <summary>
    /// Reads binary data as struct type values
    /// </summary>
    public class StructureReader : BinaryReader
    {
        public StructureReader(Stream inputStream) : base(inputStream)
        { }

        /// <summary>
        /// Read bytes from file and return it's as struct
        /// </summary>
        /// <typeparam name="T">Structure type to read</typeparam>
        /// <returns>Structure flled with read data</returns>
        public T ReadStruct<T>() where T : struct
        {
            var readStructBytes = ReadBytes(Marshal.SizeOf<T>());
            var handle = GCHandle.Alloc(readStructBytes, GCHandleType.Pinned);
            var filledStruct = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return filledStruct;
        }
    }
}
