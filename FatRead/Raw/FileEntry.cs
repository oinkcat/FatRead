using System;
using System.Collections.Generic;
using System.Text;

namespace FatRead.Raw
{
    /// <summary>
    /// Информация об элементе каталога
    /// </summary>
    public class FileEntry
    {
        public const int ShortNameLength = 11;

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

        public UInt32 Size { get; set; }
    }
}
