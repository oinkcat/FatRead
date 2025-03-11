using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FatRead.Raw;

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
        [Theory]
        [InlineData(FatType.Fat12)]
        [InlineData(FatType.Fat16)]
        [InlineData(FatType.Fat32)]
        public void TestReadRootDirectoryEntries(FatType type)
        {
            const string RootDirPath = "\\";

            using var fsImage = new FatImage(TestImageFiles.TestImagePathByType[type]);
            fsImage.ParseFat();

            var rootDirectory = fsImage.GetEntryByPath(RootDirPath);
            var entries = fsImage.EnumerateDirectory(rootDirectory).ToList();
            Assert.NotEmpty(entries);
        }

        /// <summary>
        /// Тест получения существующего файла по его пути
        /// </summary>
        [Theory]
        [InlineData(FatType.Fat16)]
        [InlineData(FatType.Fat32)]
        public void TestGetExistingFileByPath(FatType type)
        {
            const string EntryPath1 = "\\info\\include\\linux\\old_unused_rtl_wireless.h";
            const string EntryPath2 = "INFO\\Include\\LINUX\\old_unused_RTL_wireless.h";

            using var fsImage = new FatImage(TestImageFiles.TestImagePathByType[type]);
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
        [Theory]
        [InlineData(FatType.Fat16)]
        [InlineData(FatType.Fat32)]
        public void TestGetNonExistingFileByPath(FatType type)
        {
            const string NonExistingEntryPath = "\\include\\linux\\test.h";

            using var fsImage = new FatImage(TestImageFiles.TestImagePathByType[type]);
            fsImage.ParseFat();

            var nonExistingEntry = fsImage.GetEntryByPath(NonExistingEntryPath);

            Assert.Null(nonExistingEntry);
        }

        /// <summary>
        /// Тестирование обхода элементов каталога
        /// </summary>
        [Theory]
        [InlineData(FatType.Fat12)]
        [InlineData(FatType.Fat16)]
        [InlineData(FatType.Fat32)]
        public void TestDirectoryEntriesWalk(FatType type)
        {
            using var fsImage = FatImage.Open(TestImageFiles.TestImagePathByType[type]);

            var listing = new List<string>() { "\\" };
            WalkIntoDirectory(fsImage, fsImage.GetEntryByPath("\\"), listing, 1);

            Assert.NotEmpty(listing);
        }

        private void WalkIntoDirectory(FatImage fsImage, DirectoryEntry dirEntry, List<string> listing, int level)
        {
            foreach (var dirContentEntry in fsImage.EnumerateDirectory(dirEntry))
            {
                listing.Add(String.Concat(String.Empty.PadLeft(level, '-'), dirContentEntry.DisplayName));

                if (dirContentEntry.IsDirectory && !dirContentEntry.IsDotted)
                {
                    WalkIntoDirectory(fsImage, dirContentEntry, listing, level + 1);
                }
            }
        }
    }
}
