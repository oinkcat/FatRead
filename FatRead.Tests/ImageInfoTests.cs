using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FatRead.Raw;
using Xunit;

namespace FatRead.Tests
{
    /// <summary>
    /// Тестирование получения информации об образе ФС FAT
    /// </summary>
    public class ImageInfoTests
    {
        private const string TestImageFilePath = "D:/Temp/fat16.img";

        /// <summary>
        /// Тест чтения загрузочного сектора FAT
        /// </summary>
        [Fact]
        public void TestReadBootSectorInfo()
        {
            using var fatReader = new FatImageReader(TestImageFilePath);
            var commonHeader = fatReader.ReadCommonInfo();

            Assert.False(commonHeader.IsFat32);
        }

        /// <summary>
        /// Тест чтения информации о ФС FAT
        /// </summary>
        [Fact]
        public void TestReadFatInfo()
        {
            using var fatReader = new FatImageReader(TestImageFilePath);
            var commonHeader = fatReader.ReadCommonInfo();

            var fsInfo = commonHeader.IsFat32
                ? fatReader.ReadFat32Info()
                : fatReader.ReadFatInfo();

            Assert.True(commonHeader.IsValid);
            Assert.IsNotType<Fat32Info>(fsInfo);
            Assert.True(fsInfo.IsSupported);
        }

        /// <summary>
        /// Тест разбора образа ФС FAT и получения базовой информации
        /// </summary>
        [Fact]
        public void TestParseFatImageBasicInfo()
        {
            using var fsImage = new FatImage(TestImageFilePath);
            fsImage.ParseFat();

            Assert.True(fsImage.IsParsed);
            Assert.Equal(FatType.Fat16, fsImage.Type);
        }

        /// <summary>
        /// Тест чтения содержимого корневого каталога
        /// </summary>
        [Fact]
        public void TestReadRootDirectoryEntries()
        {
            const string RootDirPath = "\\"; 

            using var fsImage = new FatImage(TestImageFilePath);
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
            const string EntryPath1 = "\\include\\ip.h";
            const string EntryPath2 = "Include\\IP.h";

            using var fsImage = new FatImage(TestImageFilePath);
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

            using var fsImage = new FatImage(TestImageFilePath);
            fsImage.ParseFat();

            var nonExistingEntry = fsImage.GetEntryByPath(NonExistingEntryPath);

            Assert.Null(nonExistingEntry);
        }
    }
}