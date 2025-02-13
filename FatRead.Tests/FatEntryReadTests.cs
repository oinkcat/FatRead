using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FatRead.Tests
{
    /// <summary>
    /// Тестирование чтения содержимого файловых элементов ФС FAT
    /// </summary>
    public class FatEntryReadTests
    {
        public FatEntryReadTests() => TestImageFiles.Unpack();

        /// <summary>
        /// Тест чтения файла из образа
        /// </summary>
        [Fact]
        public void TestReadFileFromImage()
        {
            const string TestFilePath = "\\info\\include\\ip.h";

            using var fsImage = new FatImage(TestImageFiles.Fat32ImagePath);
            fsImage.ParseFat();

            var foundFile = fsImage.GetEntryByPath(TestFilePath);
            using var entryStream = fsImage.OpenFileForRead(foundFile);

            Assert.True(entryStream.CanRead);

            var entryBytes = new byte[foundFile.ContentSize];
            int bytesRead = entryStream.Read(entryBytes, 0, entryBytes.Length);

            Assert.Equal((int)foundFile.ContentSize, bytesRead);

            // Сохранить прочитанные данные
            string outFileName = Path.GetTempFileName();
            Console.WriteLine($"[TEST_OUT_FILE]: {outFileName}");

            using var testOutStream = File.Create(outFileName);
            testOutStream.Write(entryBytes);
            testOutStream.Flush();
        }
    }
}
