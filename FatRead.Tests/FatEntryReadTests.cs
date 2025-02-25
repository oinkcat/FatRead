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
            const string TestFilePath = "\\bin\\expand.exe";

            using var fsImage = new FatImage(TestImageFiles.Fat12ImagePath);
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

        /// <summary>
        /// Тест чтения данных из файла с установками позиций чтения
        /// </summary>
        [Fact]
        public void TestReadFileWithSeeks()
        {
            const string TestFilePath = "\\info\\include\\ieee80211.h";

            const int NumBytesToRead = 3;

            (SeekOrigin, long, string)[] testData =
            {
                (SeekOrigin.Begin, 0x33f7, "75-31-36"),
                (SeekOrigin.Current, 0x11ab, "63-29-20"),
                (SeekOrigin.End, -0x20c7, "6D-61-78")
            };

            using var fsImage = FatImage.Open(TestImageFiles.Fat16ImagePath);
            var fileToRead = fsImage.GetEntryByPath(TestFilePath);
            using var testFileStream = fsImage.OpenFileForRead(fileToRead);

            Assert.True(testFileStream.CanSeek);

            foreach(var (seekOrigin, offset, expectedBytes) in testData)
            {
                testFileStream.Seek(offset, seekOrigin);
                var buffer = new byte[NumBytesToRead];
                int bytesRead = testFileStream.Read(buffer);

                Assert.Equal(NumBytesToRead, bytesRead);
                Assert.Equal(expectedBytes, BitConverter.ToString(buffer));
            }
        }
    }
}
