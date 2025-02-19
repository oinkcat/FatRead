using System;
using System.Collections.Generic;
using System.IO;
using FatRead.Raw;

namespace FatRead
{
    /// <summary>
    /// Поток данных файла образа ФС FAT
    /// </summary>
    public class FatFileStream : Stream
    {
        private readonly DirectoryEntry fileEntry;

        private readonly FatImageReader reader;

        private readonly List<UInt32> fileClusters;

        private long position;

        private UInt32 currentCluster;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => fileEntry.ContentSize;

        public override long Position 
        { 
            get => position; 
            set => Seek(value, SeekOrigin.Begin); 
        }

        public FatFileStream(DirectoryEntry fileEntry, FatImageReader reader)
        {
            currentCluster = fileEntry.Cluster;
            fileClusters = new List<UInt32> { currentCluster };

            this.fileEntry = fileEntry;
            this.reader = reader;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            position = origin switch {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Math.Min(position + offset, Length),
                SeekOrigin.End => Math.Max(Length + offset, 0),
                _ => throw new ArgumentException("Incorrect offset or origin")
            };

            int finalClusterIdx = (int)(position / reader.Context.BytesPerCluster);
            int finalClusterOffset = (int)(position % reader.Context.BytesPerCluster);

            if(finalClusterIdx >= fileClusters.Count)
            {
                int chainLength = finalClusterIdx - fileClusters.Count + 1;
                var chainPart = reader.ReadFatClusterChain(fileClusters[^1], chainLength);
                fileClusters.AddRange(chainPart);

                currentCluster = fileClusters[finalClusterIdx];
            }

            reader.SeekCluster(currentCluster, (UInt16)finalClusterOffset);

            return position;
        }

        /// <summary>
        /// Прочитать байты данных файла образа ФС FAT
        /// </summary>
        /// <param name="buffer">Буфер, куда считывать данные</param>
        /// <param name="offset">Смещение от начала буфера</param>
        /// <param name="count">Число байт для чтения</param>
        /// <returns>Число прочитанных байт</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if(position >= Length) { return 0; }

            int totalBytesRead = 0;
            UInt16 clusterOffset = (UInt16)(position % reader.Context.BytesPerCluster);
            reader.SeekCluster(currentCluster, clusterOffset);

            while(totalBytesRead < count)
            {
                // Проверка на конец файла
                long bytesTillFileEnd = fileEntry.ContentSize - position;
                if(bytesTillFileEnd <= 0) { return (int)totalBytesRead;  }

                // Сколько байт считать
                UInt32 clusterBytesLeft = reader.Context.BytesPerCluster - clusterOffset;
                UInt32 clusterBytesToRead = (UInt32)Math.Min(count, clusterBytesLeft);
                clusterBytesToRead = (UInt32)Math.Min(clusterBytesToRead, bytesTillFileEnd);

                int bufferOffset = offset + totalBytesRead;
                int bytesRead = reader.RawRead(buffer, bufferOffset, (int)clusterBytesToRead);
                totalBytesRead += bytesRead;
                position += bytesRead;
                clusterOffset = 0;

                // Перейти к следующему кластеру, если достигнут конец текущего
                if(clusterBytesLeft - bytesRead <= 0)
                {
                    currentCluster = reader.LookupFatTable(currentCluster);
                    fileClusters.Add(currentCluster);
                    reader.SeekCluster(currentCluster);
                }
            }

            return totalBytesRead;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }
    }
}
