using System;
using System.Collections.Generic;
using System.Text;

namespace FatRead.Raw
{
    /// <summary>
    /// Информация о FAT32
    /// </summary>
    public class Fat32Info
    {
        public const int ReservedBytesCount = 12;

        public UInt32 FatSize32 { get; set; }

        public UInt16 ExtendedFlags { get; set; }

        public UInt16 FsVersion { get; set; }

        public UInt32 RootCluster { get; set; }

        public UInt16 FsInfoSector { get; set; }

        public UInt16 BackupSector { get; set; }

        public byte[] Reserved { get; set; }

        public Fat16Info Inherited { get; set; }
    }
}
