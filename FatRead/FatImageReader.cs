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

        private readonly BinaryReader reader;

        private FatContext context;

        private bool disposedValue;

        /// <summary>
        /// �������� ��
        /// </summary>
        internal FatContext Context 
        { 
            get => context; 
            set
            {
                context = value;
                CheckMediaTypeOfFat();
            }
        }

        public FatImageReader(string path)
        {
            imgFilePath = path;
            reader = new BinaryReader(File.OpenRead(imgFilePath));
        }

        /// <summary>
        /// ��������� ���������� �� ������ ��
        /// </summary>
        /// <returns>��������� ������������ �������</returns>
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
        /// ��������� ���������� � �� FAT (16)
        /// </summary>
        /// <returns>����� ���������� � FAT</returns>
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
        /// ��������� ���������� � �� FAT32
        /// </summary>
        /// <returns>��������� � FAT32</returns>
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
        /// ��������� ���������� �� �������� ��������
        /// </summary>
        /// <returns>���������� �������� ��������</returns>
        public DirectoryEntry ReadDirectoryEntry() => new DirectoryEntry
        {
            ShortName = Encoding.ASCII
                .GetString(reader.ReadBytes(DirectoryEntry.ShortNameLength))
                .TrimEnd('\x00', '\x20'),
            Attributes = (EntryAttribute)reader.ReadByte(),
            Reserved = reader.ReadByte(),
            CreateTimeMs = reader.ReadByte(),
            CreateTime = reader.ReadUInt16(),
            CreateDate = reader.ReadUInt16(),
            AccessDate = reader.ReadUInt16(),
            ClusterHigh = reader.ReadUInt16(),
            WriteTime = reader.ReadUInt16(),
            WriteDate = reader.ReadUInt16(),
            ClusterLow = reader.ReadUInt16(),
            ContentSize = reader.ReadUInt32()
        };

        // ��������� ��� �������� ������� FAT
        private void CheckMediaTypeOfFat()
        {
            reader.BaseStream.Seek(context.FatTableOffset, SeekOrigin.Begin);
            byte fatMediaType = reader.ReadByte();

            if(fatMediaType != context.MediaType)
            {
                throw new Exception("Invalid media type in FAT");
            }
        }

        /// <summary>
        /// ���������� �������� ������ �� ������ �������� � �������������� ��������
        /// </summary>
        /// <param name="cluster">����� ��������</param>
        /// <param name="offset">�������� ������������ ������ ��������</param>
        public void SeekCluster(UInt32 cluster, UInt16 offset = 0)
        {
            UInt32 clusterOffset;

            if(cluster > 1)
            {
                UInt16 rootDirSize = (UInt16)(context.MaxDirectoryEntries * DirectoryEntry.Size);
                UInt32 dataOffset = context.BytesPerCluster * (cluster - 2);
                clusterOffset = context.RootDirectoryOffset + rootDirSize + dataOffset;
            }
            else
            {
                clusterOffset = context.RootDirectoryOffset;
            }

            reader.BaseStream.Position = clusterOffset + offset;
        }

        /// <summary>
        /// ���������� �������� ��������� ������ � ������� FAT
        /// </summary>
        /// <param name="entry">����� ������</param>
        /// <returns>�������� �������� ������ � ������� FAT</returns>
        public UInt32 LookupFatTable(UInt32 entry)
        {
            ThrowIfNoContext();

            long origOffset = reader.BaseStream.Position;

            int entrySize = Context.IsFat32 ? sizeof(UInt32) : sizeof(UInt16);
            long newOffset = Context.FatTableOffset + entry * entrySize;
            reader.BaseStream.Position = newOffset;

            UInt32 readValue = Context.IsFat32 
                ? reader.ReadUInt32() & 0x0fffffff 
                : reader.ReadUInt16();

            reader.BaseStream.Position = origOffset;

            return readValue;
        }

        private void ThrowIfNoContext()
        {
            if(Context == null)
            {
                throw new Exception("FAT Context not given");
            }
        }

        /// <summary>
        /// ������� ���� ������
        /// </summary>
        public void Close()
        {
            reader?.Close();
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