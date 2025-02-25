using System;
using System.Collections.Generic;
using System.Text;

namespace FatRead.Raw
{
    /// <summary>
    /// Тип файловой системы FAT
    /// </summary>
    public enum FatType
    {
        Unsupported,
        Fat12,
        Fat16,
        Fat32
    }

    /// <summary>
    /// Общий заголовок файловой системы FAT
    /// </summary>
    public class FatCommonHeader
    {
        private const int MaxDataClustersFat12 = 4084;

        private const int MaxDataClustersFat16 = 65524;

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
        public bool IsFat32 => GuessedType == FatType.Fat32;

        /// <summary>
        /// Предполагаемый тип файловой системы FAT
        /// </summary>
        public FatType GuessedType { get; private set; }

        /// <summary>
        /// Попытаться определить тип файловой системы
        /// </summary>
        public void GuessFatType()
        {
            if (!IsValid) { return; }

            if (FatSize16 != 0 && (TotalSectors16 != 0 || TotalSectors32 != 0))
            {
                UInt32 totalSectors = (TotalSectors16 == 0) ? TotalSectors32 : TotalSectors16;
                UInt32 rootDirSectors = NumberOfRootEntries * DirectoryEntry.Size / BytesPerSector;
                UInt32 fatSectors = (UInt32)(ReservedCount + NumberOfFats * FatSize16);
                UInt32 dataSectors = totalSectors - fatSectors + rootDirSectors;
                UInt32 clustersCount = dataSectors / SectorsPerCluster;

                GuessedType = (clustersCount <= MaxDataClustersFat12)
                    ? FatType.Fat12
                    : ((clustersCount <= MaxDataClustersFat16) ? FatType.Fat16 : FatType.Fat32);
            } else if(FatSize16 == 0)
            {
                GuessedType = FatType.Fat32;
            }
        }
    }
}
