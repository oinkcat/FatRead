using System;
using System.Collections.Generic;
using System.Text;
using FatRead.Raw;

namespace FatRead
{
    /// <summary>
    /// Информация о ФС FAT, необходимая для чтения файлов
    /// </summary>
    internal class FatContext
    {
        private const UInt32 EndOfClusterChainFat12Value = 0xff8;

        private const UInt32 EndOfClusterChainFat16Value = 0xfff8;

        private const UInt32 EndOfClusterChainFat32Value = 0x0ffffff8;

        /// <summary>
        /// Тип файловой системы
        /// </summary>
        public FatType Type { get; set; }

        /// <summary>
        /// ФС FAT является 32-х разрядной
        /// </summary>
        public bool IsFat32 { get; set; }

        /// <summary>
        /// Тип носителя
        /// </summary>
        public byte MediaType { get; set; }

        /// <summary>
        /// Число байт на сектор
        /// </summary>
        public UInt32 BytesPerCluster { get; set; }

        /// <summary>
        /// Смещение таблицы FAT
        /// </summary>
        public UInt32 FatTableOffset { get; set; }

        /// <summary>
        /// Смещение корневого каталога
        /// </summary>
        public UInt32 RootDirectoryOffset { get; set; }

        /// <summary>
        /// Максимальное число элементов каталога
        /// </summary>
        public UInt16 MaxDirectoryEntries { get; set; }

        /// <summary>
        /// Создать из информации о ФС
        /// </summary>
        /// <param name="bootSector">Информация загрузочного сектора</param>
        /// <param name="fatInfo">Информация о ФС FAT</param>
        /// <returns>Контекст ФС</returns>
        public static FatContext FromFsInfos(FatCommonHeader bootSector, FatInfo fatInfo)
        {
            UInt32 fatSize = (fatInfo is Fat32Info fat32Info)
                ? fat32Info.FatSize32 * bootSector.BytesPerSector
                : (UInt32)bootSector.FatSize16 * bootSector.BytesPerSector;

            UInt32 fatOffset = (UInt32)bootSector.BytesPerSector * bootSector.ReservedCount;
            UInt32 totalFatsBytes = fatSize * bootSector.NumberOfFats;

            return new FatContext
            {
                Type = bootSector.GuessedType,
                IsFat32 = bootSector.IsFat32,
                MediaType = bootSector.MediaType,
                BytesPerCluster = (UInt32)bootSector.BytesPerSector * bootSector.SectorsPerCluster,
                FatTableOffset = fatOffset,
                RootDirectoryOffset = fatOffset + totalFatsBytes,
                MaxDirectoryEntries = bootSector.NumberOfRootEntries
            };
        }

        /// <summary>
        /// Получить число байт в указанном кластере
        /// </summary>
        /// <param name="cluster">Номер кластера</param>
        /// <returns>Число доступных для чтения байт</returns>
        public UInt32 GetBytesForCluster(UInt32 cluster) => (IsFat32 || cluster > 1)
                ? BytesPerCluster
                : MaxDirectoryEntries * DirectoryEntry.Size;

        /// <summary>
        /// Является ли номер кластера признаком конца цепочки
        /// </summary>
        /// <param name="cluster">Номер кластера</param>
        /// <returns>Признак конца цепочки кластеров</returns>
        public bool IsEndOfChain(UInt32 cluster) => cluster >= Type switch
        {
            FatType.Fat12 => EndOfClusterChainFat12Value,
            FatType.Fat16 => EndOfClusterChainFat16Value,
            FatType.Fat32 => EndOfClusterChainFat32Value,
            _ => throw new ArgumentException(nameof(Type))
        };
    }
}
