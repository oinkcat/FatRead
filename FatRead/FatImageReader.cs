using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using FatRead.Raw;

namespace FatRead
{
    /// <summary>
    /// Reads FAT filesystem image
    /// </summary>
    public class FatImageReader : IDisposable
    {
        private readonly string imgFilePath;

        private BinaryReader reader;
        private bool disposedValue;

        /// <summary>
        /// Структуры ФС разобраны
        /// </summary>
        public bool IsParsed { get; private set; }

        public FatImageReader(string path)
        {
            imgFilePath = path;
            reader = new BinaryReader(File.OpenRead(imgFilePath));
        }

        /// <summary>
        /// Прочитать информацию об образе ФС
        /// </summary>
        /// <returns>Заголовок загрузочного сектора</returns>
        public FatCommonHeader ReadCommonInfo() => new FatCommonHeader
        {
            JmpCommand = reader.ReadBytes(FatCommonHeader.JmpCommandLength),
            OemName = Encoding.ASCII.GetString(reader.ReadBytes(FatCommonHeader.OemNameLength)),
            BytesPerSector = reader.ReadUInt16(),
            SectorsPerCluster = reader.ReadByte(),
            ReservedCount = reader.ReadUInt16(),
            NumberOfFats = reader.ReadByte(),
            NumberOfRootEntries = reader.ReadUInt16(),
            TotalSectors16 = reader.ReadUInt16(),
            MediaType = reader.ReadByte(),
            FatSize16 = reader.ReadUInt16(),
            SectorsPerTrack = reader.ReadUInt16(),
            NumberOfHeads = reader.ReadUInt16(),
            NumberOfHiddenSectors = reader.ReadUInt32(),
            TotalSectors32 = reader.ReadUInt32()
        };

        /// <summary>
        /// Прочитать информацию о ФС FAT (16)
        /// </summary>
        /// <returns>Общая информация о FAT</returns>
        public FatInfo ReadFatInfo()
        {
            var fatInfo = new FatInfo();
            FillFatInfo(fatInfo);

            return fatInfo;
        }

        private void FillFatInfo(FatInfo info)
        {
            info.DriveNumber = reader.ReadByte();
            info.Reserved1 = reader.ReadByte();
            info.BootSignature = reader.ReadByte();
            info.VolumeId = reader.ReadUInt32();
            info.VolumeLabel = Encoding.ASCII.GetString(reader.ReadBytes(FatInfo.LabelLength));
            info.FsType = Encoding.ASCII.GetString(reader.ReadBytes(FatInfo.FsTypeLength));
        }

        /// <summary>
        /// Прочитать информацию о ФС FAT32
        /// </summary>
        /// <returns>Информаия о FAT32</returns>
        public Fat32Info ReadFat32Info()
        {
            var fat32Info = new Fat32Info
            {
                FatSize32 = reader.ReadUInt32(),
                ExtendedFlags = reader.ReadUInt16(),
                FsVersion = reader.ReadUInt16(),
                RootCluster = reader.ReadUInt32(),
                FsInfoSector = reader.ReadUInt16(),
                BackupSector = reader.ReadUInt16(),
                Reserved = reader.ReadBytes(Fat32Info.ReservedBytesCount)
            };

            FillFatInfo(fat32Info);

            return fat32Info;
        }

        /// <summary>
        /// Прочитать информацию об элементе каталога
        /// </summary>
        /// <returns>Информация элемента каталога</returns>
        public DirectoryEntry ReadDirectoryEntry() => new DirectoryEntry
        {
            ShortName = Encoding.ASCII.GetString(reader.ReadBytes(DirectoryEntry.ShortNameLength)),
            Attributes = reader.ReadByte(),
            Reserved = reader.ReadByte(),
            CreateTimeMs = reader.ReadByte(),
            CreateTime = reader.ReadUInt16(),
            CreateDate = reader.ReadUInt16(),
            AccessDate = reader.ReadUInt16(),
            ClusterHigh = reader.ReadUInt16(),
            WriteTime = reader.ReadUInt16(),
            WriteDate = reader.ReadUInt16(),
            ClusterLow = reader.ReadUInt16(),
            Size = reader.ReadUInt32()
        };

        /// <summary>
        /// Закрыть файл образа
        /// </summary>
        public void Close()
        {
            if(reader != null)
            {
                reader.Close();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}