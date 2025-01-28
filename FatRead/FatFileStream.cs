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

        private long position;

        private UInt32 currentCluster;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => fileEntry.ContentSize;

        public override long Position 
        { 
            get => position; 
            set => throw new NotImplementedException(); 
        }

        public FatFileStream(DirectoryEntry fileEntry, FatImageReader reader)
        {
            currentCluster = fileEntry.Cluster;
            this.fileEntry = fileEntry;
            this.reader = reader;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            // TODO!
            throw new NotImplementedException();
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
