using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FatRead.Raw;

namespace FatRead
{
    /// <summary>
    /// Тип файловой системы FAT
    /// </summary>
    public enum FatType
    {
        Unsupported,
        Fat16,
        Fat32
    }

    /// <summary>
    /// Образ файловой системы FAT
    /// </summary>
    public class FatImage : IDisposable
    {
        private readonly string filePath;

        private readonly FatImageReader reader;

        private FatCommonHeader bootSector;

        private FatInfo info;

        private FatContext context;

        private bool disposedValue;

        /// <summary>
        /// Тип ФС
        /// </summary>
        public FatType Type { get; private set; }

        /// <summary>
        /// Структуры ФС разобраны
        /// </summary>
        public bool IsParsed { get; private set; }

        public FatImage(string path)
        {
            filePath = path;
            reader = new FatImageReader(filePath);
        }

        /// <summary>
        /// Прочитать информацию об образе FAT
        /// </summary>
        public void ParseFat()
        {
            bootSector = reader.ReadCommonInfo();
            info = bootSector.IsFat32 ? reader.ReadFat32Info() : reader.ReadFatInfo();
            context = FatContext.FromFsInfos(bootSector, info);

            Type = info.IsSupported
                ? (context.IsFat32 ? FatType.Fat32 : FatType.Fat16)
                : FatType.Unsupported;

            reader.Context = context;

            IsParsed = true;
        }

        /// <summary>
        /// Получить информацию о элементе по его пути
        /// </summary>
        /// <param name="entryPath">Путь к элементу</param>
        /// <returns>Элемент ФС - файл или каталог</returns>
        public DirectoryEntry GetEntryByPath(string entryPath)
        {
            const char PathSeparator = '\\';
            const StringComparison IgnoreCase = StringComparison.InvariantCultureIgnoreCase;
            const StringSplitOptions IgnoreEmpty = StringSplitOptions.RemoveEmptyEntries;

            string cleanEntryPath = (entryPath == PathSeparator.ToString()) 
                ? entryPath 
                : entryPath.Trim(PathSeparator);

            if(String.IsNullOrEmpty(cleanEntryPath))
            {
                throw new Exception("The path is empty");
            }

            var directoryToLookup = DirectoryEntry.RootDirectory;
            var pathParts = new Queue<string>(cleanEntryPath.Split(PathSeparator, IgnoreEmpty));

            while(pathParts.TryDequeue(out string pathPartName))
            {
                bool partIsFound = false;

                foreach(var dirEntry in EnumerateDirectory(directoryToLookup))
                {
                    if(dirEntry.DisplayShortName.Equals(pathPartName, IgnoreCase))
                    {
                        partIsFound = true;

                        if (pathParts.Any())
                        {
                            directoryToLookup = dirEntry;
                            break;
                        }
                        else
                        {
                            return dirEntry;
                        }
                    }
                }

                if(!partIsFound) { return null; }
            }

            return DirectoryEntry.RootDirectory;
        }

        /// <summary>
        /// Получить список записей элементов каталога
        /// </summary>
        /// <param name="directory">Каталог для просмотра</param>
        /// <returns>Записи элементов каталога</returns>
        public IEnumerable<DirectoryEntry> EnumerateDirectory(DirectoryEntry directory)
        {
            if(!directory.IsDirectory)
            {
                throw new Exception("Entry is not a directory");
            }

            UInt32 dirCluster = directory.ClusterLow;

            UInt32 getTotalBytesToRead() => (context.IsFat32 || dirCluster > 1)
                ? context.BytesPerCluster
                : context.MaxDirectoryEntries * DirectoryEntry.Size;

            reader.SeekCluster(dirCluster);

            UInt32 totalBytesRead = 0;
            bool reading = true;
            UInt32 bytesToRead = getTotalBytesToRead();

            while (reading)
            {
                var readEntry = reader.ReadDirectoryEntry();
                totalBytesRead += DirectoryEntry.Size;
                reading = readEntry.ShortName.Length > 0;

                if (reading)
                {
                    if (!readEntry.IsLfnEntry)
                    {
                        yield return readEntry;
                    }

                    if (reading && (totalBytesRead >= bytesToRead))
                    {
                        // Перейти к следующему кластеру списка элементов каталога
                        dirCluster = reader.LookupFatTable(dirCluster);
                        reader.SeekCluster(dirCluster);

                        bytesToRead = getTotalBytesToRead();
                        totalBytesRead = 0;
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    reader?.Dispose();
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
