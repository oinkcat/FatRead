using System;

namespace FatRead
{
    /// <summary>
    /// FAT essential information
    /// </summary>
    internal class FatInfo
    {
        public string FormattedOsName { get; set; }

        public int BytesPerSector { get; set; }

        public int NumberOfRootEntries { get; set; }
    }
}