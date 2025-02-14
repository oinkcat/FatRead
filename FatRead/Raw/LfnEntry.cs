using System;
using System.Collections.Generic;
using System.Text;

namespace FatRead.Raw
{
    /// <summary>
    /// Информация о части длинного имени файла
    /// </summary>
    public class LfnEntry
    {
        internal const int NameFirstPartLength = 10;

        internal const int NameSecondPartLength = 12;

        internal const int NameThirdPartLength = 4;

        public byte SequenceNumber { get; set; }

        public string NameFirstPart { get; set; }

        public EntryAttribute Attributes { get; set; }

        public byte SfnChecksum { get; set; }

        public string NameSecondPart { get; set; }

        public UInt16 FirstCluster { get; set; }

        public string NameThirdPart { get; set; }

        /// <summary>
        /// Индекс элемента имени
        /// </summary>
        public int Index => SequenceNumber & 0x0f;
        
        /// <summary>
        /// Создать из информации о записи каталога
        /// </summary>
        /// <param name="entry">Запись каталога ФС FAT</param>
        /// <returns>Запись части длинного имени файла</returns>
        public static LfnEntry CreateFromDirectoryEntry(DirectoryEntry entry)
        {
            var rawBytes = new byte[DirectoryEntry.Size];

            Encoding.ASCII.GetBytes(entry.ShortName).CopyTo(rawBytes, 0);
            rawBytes[0x0b] = (byte)entry.Attributes;
            rawBytes[0x0c] = entry.Reserved;
            rawBytes[0x0d] = entry.CreateTimeMs;
            BitConverter.GetBytes(entry.CreateTime).CopyTo(rawBytes, 0x0e);
            BitConverter.GetBytes(entry.CreateDate).CopyTo(rawBytes, 0x10);
            BitConverter.GetBytes(entry.AccessDate).CopyTo(rawBytes, 0x12);
            BitConverter.GetBytes(entry.ClusterHigh).CopyTo(rawBytes, 0x14);
            BitConverter.GetBytes(entry.WriteTime).CopyTo(rawBytes, 0x16);
            BitConverter.GetBytes(entry.WriteDate).CopyTo(rawBytes, 0x18);
            BitConverter.GetBytes(entry.ClusterLow).CopyTo(rawBytes, 0x1a);
            BitConverter.GetBytes(entry.ContentSize).CopyTo(rawBytes, 0x1c);

            return new LfnEntry
            {
                SequenceNumber = rawBytes[0],
                NameFirstPart = Encoding.Unicode.GetString(rawBytes, 0x01, NameFirstPartLength),
                Attributes = (EntryAttribute)rawBytes[0x0b],
                SfnChecksum = rawBytes[0x0d],
                NameSecondPart = Encoding.Unicode.GetString(rawBytes, 0x0e, NameSecondPartLength),
                FirstCluster = BitConverter.ToUInt16(rawBytes, 0x1a),
                NameThirdPart = Encoding.Unicode.GetString(rawBytes, 0x1c, NameThirdPartLength)
            };
        }
    }
}
