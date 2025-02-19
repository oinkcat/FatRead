using System;
using System.Collections.Generic;
using System.Text;

namespace FatRead.Raw
{
    /// <summary>
    /// Общий заголовок файловой системы FAT
    /// </summary>
    public class FatCommonHeader
    {
        private const byte HexCodeJmp = 0xeb;

        private const byte HexCodeNop = 0x90;

        public const int JmpCommandLength = 3;

        public const int OemNameLength = 8;

        public byte[] JmpCommand { get; set; }

        public string OemName { get; set; }

        public UInt16 BytesPerSector { get; set; }

        public byte SectorsPerCluster { get; set; }

        public UInt16 ReservedCount { get; set; }

        public byte NumberOfFats { get; set; }

        public UInt16 NumberOfRootEntries { get; set; }

        public UInt16 TotalSectors16 { get; set; }

        public byte MediaType { get; set; }

        public UInt16 FatSize16 { get; set; }

        public UInt16 SectorsPerTrack { get; set; }

        public UInt16 NumberOfHeads { get; set; }

        public UInt32 NumberOfHiddenSectors { get; set; }

        public UInt32 TotalSectors32 { get; set; }

        /// <summary>
        /// Представляет действительный загрузочный сектор
        /// </summary>
        public bool IsValid => (JmpCommand[0] == HexCodeJmp) && (JmpCommand[2] == HexCodeNop);

        /// <summary>
        /// Является ли тип ФС FAT32
        /// </summary>
        /// <remarks>TODO: Не совсем правильное определение, нужно исправить</remarks>
        public bool IsFat32 => FatSize16 == 0;
    }
}
