using System;
using System.Collections.Generic;
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
