using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FatRead.Tests
{
    /// <summary>
    /// Тестирование получения информации об элементах каталогов ФС FAT
    /// </summary>
    public class FatDirectoryEntryTests
    {
        public FatDirectoryEntryTests() => TestImageFiles.Unpack();

        /// <summary>
        /// Тест чтения содержимого корневого каталога
        /// </summary>
        [Fact]
        public void TestReadRootDirectoryEntries()
        {
            const string RootDirPath = "\\";

            using var fsImage = new FatImage(TestImageFiles.Fat32ImagePath);
            fsImage.ParseFat();

            var rootDirectory = fsImage.GetEntryByPath(RootDirPath);
            var entries = fsImage.EnumerateDirectory(rootDirectory).ToList();
            Assert.NotEmpty(entries);
        }

        /// <summary>
        /// Тест получения существующего файла по его пути
        /// </summary>
        [Fact]
        public void TestGetExistingFileByPath()
        {
            const string EntryPath1 = "\\info\\include\\ip.h";
            const string EntryPath2 = "info\\Include\\IP.h";

            using var fsImage = new FatImage(TestImageFiles.Fat32ImagePath);
            fsImage.ParseFat();

            var foundEntry1 = fsImage.GetEntryByPath(EntryPath1);
            var foundEntry2 = fsImage.GetEntryByPath(EntryPath2);

            Assert.NotNull(foundEntry1);
            Assert.NotNull(foundEntry2);
            Assert.Equal(foundEntry1.ClusterHigh, foundEntry2.ClusterHigh);
            Assert.Equal(foundEntry1.ClusterLow, foundEntry2.ClusterLow);
        }

        /// <summary>
        /// Тест получения несуществующего файла по пути
        /// </summary>
        [Fact]
        public void TestGetNonExistingFileByPath()
        {
            const string NonExistingEntryPath = "\\include\\linux\\test.h";

            using var fsImage = new FatImage(TestImageFiles.Fat32ImagePath);
            fsImage.ParseFat();

            var nonExistingEntry = fsImage.GetEntryByPath(NonExistingEntryPath);

            Assert.Null(nonExistingEntry);
        }
    }
}
