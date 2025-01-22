using System;
using System.Collections.Generic;
using System.Text;

namespace FatRead.Raw
{
    /// <summary>
    /// Информация об элементе каталога
    /// </summary>
    public class DirectoryEntry
    {
        /// <summary>
        /// Размер элемента в байтах
        /// </summary>
        public const UInt32 Size = 32;

        public const int ShortNameLength = 11;

        public const int EmptyEntryName = 0xE5;

        public string ShortName { get; set; }

        public byte Attributes { get; set; }

        public byte Reserved { get; set; }

        public byte CreateTimeMs { get; set; }

        public UInt16 CreateTime { get; set; }

        public UInt16 CreateDate { get; set; }

        public UInt16 AccessDate { get; set; }

        public UInt16 ClusterHigh { get; set; }

        public UInt16 WriteTime { get; set; }

        public UInt16 WriteDate { get; set; }

        public UInt16 ClusterLow { get; set; }

        public UInt32 ContentSize { get; set; }
    }
}
