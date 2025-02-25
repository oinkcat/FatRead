using System;
using System.Collections.Generic;
using System.Text;

namespace FatRead.Raw
{
    /// <summary>
    /// Информация о FAT16 и FAT32
    /// </summary>
    public class FatInfo
    {
        public const int LabelLength = 11;

        public const int FsTypeLength = 8;

        public byte DriveNumber { get; set; }

        public byte Reserved1 { get; set; }

        public byte BootSignature { get; set; }

        public UInt32 VolumeId { get; set; }

        public string VolumeLabel { get; set; }

        public string FsType { get; set; }
    }
}
