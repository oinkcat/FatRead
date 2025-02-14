using System;
using System.Collections.Generic;
using System.Text;

namespace FatRead.Raw
{
    /// <summary>
    /// Атрибут элемента каталога
    /// </summary>
    [Flags]
    public enum EntryAttribute : byte
    {
        Readonly = 0x01,
        Hidden = 0x02,
        System = 0x04,
        VolumeId = 0x08,
        Directory = 0x10,
        Archive = 0x20
    }

    /// <summary>
    /// Информация об элементе каталога
    /// </summary>
    public class DirectoryEntry
    {
        /// <summary>
        /// Разделитель пути
        /// </summary>
        public const char PathSeparator = '\\';

        /// <summary>
        /// Размер элемента в байтах
        /// </summary>
        internal const UInt32 Size = 32;

        internal const int ShortNameLength = 11;

        internal const byte EmptyEntryName = 0xE5;

        /// <summary>
        /// Корневой каталог
        /// </summary>
        internal static DirectoryEntry RootDirectory => new DirectoryEntry
        {
            ShortName = PathSeparator.ToString(),
            Attributes = EntryAttribute.Directory | EntryAttribute.System,
            ClusterLow = 1
        };

        private string longName;

        public string ShortName { get; set; }

        public EntryAttribute Attributes { get; set; }

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

        /// <summary>
        /// Является ли запись пустой
        /// </summary>
        public bool IsFree => ShortName[0] == (char)EmptyEntryName;

        /// <summary>
        /// Является ли каталогом
        /// </summary>
        public bool IsDirectory => Attributes.HasFlag(EntryAttribute.Directory);

        /// <summary>
        /// Является элементом-ссылкой на текущий или предыдущие каталоги
        /// </summary>
        public bool IsDotted => DisplayShortName.Equals(".") || DisplayShortName.Equals("..");

        /// <summary>
        /// Номер кластера
        /// </summary>
        public UInt32 Cluster => (UInt32)ClusterHigh << 16 | ClusterLow;

        /// <summary>
        /// Отображаемое короткое имя
        /// </summary>
        public string DisplayShortName
        {
            get
            {
                const int ShortNamePartLength = 8;

                if(ShortName.Length > ShortNamePartLength)
                {
                    string namePart = ShortName[..ShortNamePartLength].TrimEnd();
                    string extPart = ShortName[ShortNamePartLength..];

                    return $"{namePart}.{extPart}";
                }
                else
                {
                    return ShortName;
                }
            }
        }

        /// <summary>
        /// Отображаемое имя
        /// </summary>
        public string DisplayName => longName ?? DisplayShortName;

        /// <summary>
        /// Является записью LFN
        /// </summary>
        public bool IsLfnEntry =>
            Attributes.HasFlag(EntryAttribute.Readonly)
            && Attributes.HasFlag(EntryAttribute.Hidden)
            && Attributes.HasFlag(EntryAttribute.System)
            && Attributes.HasFlag(EntryAttribute.VolumeId);

        /// <summary>
        /// Присвоить длинное имя
        /// </summary>
        /// <param name="lfnParts">Части длинного имени файла</param>
        public void AssignLfn(IList<LfnEntry> lfnParts)
        {
            var longNameBuffer = new StringBuilder();

            for(int i = lfnParts.Count - 1; i >= 0; i--)
            {
                var part = lfnParts[i];
                longNameBuffer.Append(part.NameFirstPart);
                longNameBuffer.Append(part.NameSecondPart);
                longNameBuffer.Append(part.NameThirdPart);
            }

            longName = longNameBuffer.ToString().Split('\0')[0];
        }
    }
}
