using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace FatRead
{
    /// <summary>
    /// Reads FAT filesystem image
    /// </summary>
    public class FatImageReader : IDisposable
    {
        private const int DefaultSectorSize = 512;

        private const string JmpCodeHex = "EB-3C-90";
        private const int JmpCodeLength = 3;

        private const int OemNameLength = 8;
        private const int VolumeLabelLength = 11;

        private const int NamePartLength = 8;
        private const int ExtensionLength = 3;

        private const int EmptyEntryName = 0xE5;

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
        }

        /// <summary>
        /// Прочитать информацию об образе ФС
        /// </summary>
        public void Read()
        {
            reader = new BinaryReader(File.OpenRead(imgFilePath));

            ReadBasicInfo();
        }

        private void ReadBasicInfo()
        {
            var headerBytes = new Byte[DefaultSectorSize];
            _ = reader.Read(headerBytes);

            string jmpCodeValue = BitConverter.ToString(headerBytes, 0, JmpCodeLength);

            if(jmpCodeValue != JmpCodeHex)
            {
                throw new Exception("Invalid header");
            }

            int offset = JmpCodeLength;
            string oemName = Encoding.ASCII.GetString(headerBytes, offset, OemNameLength);
            Console.WriteLine(oemName);
        }

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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}