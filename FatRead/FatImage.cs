using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Открыть файл образа для чтения
        /// </summary>
        /// <param name="filePath">Путь к файлу образа</param>
        /// <returns>Образ файловой системы FAT</returns>
        public static FatImage Open(string filePath)
        {
            var fatImage = new FatImage(filePath);
            fatImage.ParseFat();

            return fatImage;
        }

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
                    if(dirEntry.DisplayName.Equals(pathPartName, IgnoreCase))
                    {
                        partIsFound = true;

                        if (pathParts.Count > 0)
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
            reader.SeekCluster(dirCluster);

            UInt32 totalBytesRead = 0;
            bool reading = true;
            UInt32 bytesToRead = context.GetBytesForCluster(dirCluster);

            var lfnParts = new List<LfnEntry>();

            while (reading)
            {
                var readEntry = reader.ReadDirectoryEntry();
                totalBytesRead += DirectoryEntry.Size;
                reading = readEntry.ShortName.Length > 0;

                if (reading)
                {
                    if (!readEntry.IsLfnEntry)
                    {
                        if(lfnParts.Count > 0)
                        {
                            readEntry.AssignLfn(lfnParts);
                            lfnParts.Clear();
                        }

                        long currentPos = reader.RawPosition;
                        yield return readEntry;

                        if(reader.RawPosition != currentPos)
                        {
                            reader.RawPosition = currentPos;
                        }
                    }
                    else
                    {
                        lfnParts.Add(LfnEntry.CreateFromDirectoryEntry(readEntry));
                    }

                    if (reading && (totalBytesRead >= bytesToRead))
                    {
                        // Перейти к следующему кластеру списка элементов каталога
                        dirCluster = reader.LookupFatTable(dirCluster);
                        reader.SeekCluster(dirCluster);

                        bytesToRead = context.GetBytesForCluster(dirCluster);
                        totalBytesRead = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Открыть запись файла ФС FAT для чтения данных
        /// </summary>
        /// <param name="entry">Запись файла</param>
        /// <returns>Поток, пригодный для чтения данных</returns>
        public FatFileStream OpenFileForRead(DirectoryEntry entry)
        {
            return new FatFileStream(entry, reader);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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
    }
}
